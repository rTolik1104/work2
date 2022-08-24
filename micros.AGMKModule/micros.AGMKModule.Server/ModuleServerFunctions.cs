﻿using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace micros.AGMKModule.Server
{
  public class ModuleFunctions
  {
    [Public, Remote(IsPure = true)]
    public static IQueryable<Sungero.Meetings.IMeeting> GetMeetings()
    {
      return Sungero.Meetings.Meetings.GetAll().AsQueryable();
    }
    
    [Public, Remote(IsPure = true)]
    public static IQueryable<Sungero.Meetings.IMinutes> GetMinutesByMeetingIds(string meetingIds, char separator)
    {
      IQueryable<Sungero.Meetings.IMinutes> result = null;
      List<int> ids = new List<int>();
      if (!string.IsNullOrEmpty(meetingIds))
      {
        foreach (var idStr in meetingIds.Split(separator))
        {
          if(int.TryParse(idStr, out int id))
            ids.Add(id);
        }
        
        if (ids != null && ids.Any())
        {
          var idArray = ids.ToArray();
          result = Sungero.Meetings.Minuteses.GetAll(m => m.Meeting != null && idArray.Contains(m.Meeting.Id));
        }
        else
          result = Sungero.Meetings.Minuteses.GetAll(m => m.Meeting != null);
      }
      return result;
    }
    
    /// <summary>
    /// Получить краткую информацию по исполнению поручений в срок за период.
    /// </summary>
    /// <param name="meeting">Совещание.</param>
    /// <param name="document">Документ.</param>
    /// <param name="beginDate">Начало периода.</param>
    /// <param name="endDate">Конец периода.</param>
    /// <param name="author">Автор.</param>
    /// <param name="businessUnit">НОР.</param>
    /// <param name="department">Подразделение.</param>
    /// <param name="performer">Исполнитель.</param>
    /// <param name="documentType">Тип документов во вложениях поручений.</param>
    /// <param name="isMeetingsCoverContext">Признак контекста вызова с обложки совещаний.</param>
    /// <param name="getCoAssignees">Признак необходимости получения соисполнителей.</param>
    /// <returns>Краткая информация по исполнению поручений в срок за период.</returns>
    public virtual List<Structures.Module.LightActiomItem> GetActionItemCompletionData(string meetingIds,
                                                                                       Sungero.Docflow.IOfficialDocument document,
                                                                                       DateTime? beginDate,
                                                                                       DateTime? endDate,
                                                                                       Sungero.Company.IEmployee author,
                                                                                       Sungero.Company.IBusinessUnit businessUnit,
                                                                                       Sungero.Company.IDepartment department,
                                                                                       IUser performer,
                                                                                       Sungero.Docflow.IDocumentType documentType,
                                                                                       bool? isMeetingsCoverContext,
                                                                                       bool getCoAssignees)
    {
      List<Structures.Module.LightActiomItem> tasks = null;
      
      IQueryable<Sungero.Meetings.IMinutes> minutesList = GetMinutesByMeetingIds(meetingIds, ';');
      
      var isAdministratorOrAdvisor = Sungero.Docflow.PublicFunctions.Module.Remote.IsAdministratorOrAdvisor();
      var recipientsIds = Substitutions.ActiveSubstitutedUsers.Select(u => u.Id).ToList();
      recipientsIds.Add(Users.Current.Id);
      
      AccessRights.AllowRead(
        () =>
        {
          var query = Sungero.RecordManagement.ActionItemExecutionTasks.GetAll()
            .Where(t => isAdministratorOrAdvisor ||
                   recipientsIds.Contains(t.Author.Id) || recipientsIds.Contains(t.StartedBy.Id) ||
                   t.ActionItemType == Sungero.RecordManagement.ActionItemExecutionTask.ActionItemType.Component &&
                   recipientsIds.Contains(t.MainTask.StartedBy.Id))
            .Where(t => t.Status == Sungero.Workflow.Task.Status.Completed || t.Status == Sungero.Workflow.Task.Status.InProcess)
            .Where(t => t.IsCompoundActionItem != true && t.ActionItemType != Sungero.RecordManagement.ActionItemExecutionTask.ActionItemType.Additional);
          
          // Если отчёт вызывается не из документа (свойство Документ не заполнено), то даты заполнены и по ним нужно фильтровать.
          // Если же отчёт вызывается из документа, то поручения нужно фильтровать по этому документу во вложении.
          // Если отчет вызывается из Совещания, то поручения нужно фильтровать по протоколам этого совещания.
          
          // Guid группы вложений для документа в поручении.
          var documentsGroupGuid = Sungero.Docflow.PublicConstants.Module.TaskMainGroup.ActionItemExecutionTask;
          
          query = query.Where(t => t.AttachmentDetails.Any(g => g.GroupId == documentsGroupGuid && minutesList.Any(m => m.Id == g.AttachmentId)));
          
          if (documentType != null)
          {
            var documents = Sungero.Docflow.OfficialDocuments.GetAll(d => d.DocumentKind.DocumentType == documentType);
            
            // В Hibernate обращаться к группам вложений задачи можно только через метаданные.
            query = query.Where(t => t.AttachmentDetails.Any(g => g.GroupId == documentsGroupGuid && documents.Any(m => m.Id == g.AttachmentId)));
          }
          
          if (document != null)
          {
            // В Hibernate обращаться к группам вложений задачи можно только через метаданные.
            query = query.Where(t => t.AttachmentDetails.Any(g => g.GroupId == documentsGroupGuid && g.AttachmentId == document.Id));
          }
          else
          {
            var serverBeginDate = Sungero.Docflow.PublicFunctions.Module.Remote.GetTenantDateTimeFromUserDay(beginDate.Value);
            var serverEndDate = endDate.Value.EndOfDay().FromUserTime();
            query = query.Where(t => t.Status == Sungero.Workflow.Task.Status.Completed &&
                                (Calendar.Between(t.ActualDate.Value.Date, beginDate.Value.Date, endDate.Value.Date) ||
                                 t.Deadline.HasValue &&
                                 ((t.Deadline.Value.Date == t.Deadline.Value ? t.Deadline.Between(beginDate.Value.Date, endDate.Value.Date) : t.Deadline.Between(serverBeginDate, serverEndDate)) ||
                                  t.ActualDate.Value.Date >= endDate && (t.Deadline.Value.Date == t.Deadline.Value ? t.Deadline <= beginDate.Value.Date : t.Deadline <= serverBeginDate))) ||
                                t.Status == Sungero.Workflow.Task.Status.InProcess && t.Deadline.HasValue &&
                                (t.Deadline.Value.Date == t.Deadline.Value ? t.Deadline <= endDate.Value.Date : t.Deadline <= serverEndDate));
          }
          
          // Dmitriev_IA: Проверка вынесена из Select для ускорения получения данных. Если занести проверку в Select, то проверка будет происходить для каждого t.
          if (getCoAssignees)
            tasks = query
              .Select(t => Structures.Module.LightActiomItem.Create(t.Id, t.Status, t.ActualDate, t.Deadline, t.Author, t.Assignee, t.ActionItem, t.ExecutionState, this.GetCoAssigneesShortNames(t), this.GetReportText(t)))
              .ToList();
          else
            tasks = query
              .Select(t => Structures.Module.LightActiomItem.Create(t.Id, t.Status, t.ActualDate, t.Deadline, t.Author, t.Assignee, t.ActionItem, t.ExecutionState, null, this.GetReportText(t)))
              .ToList();
        });
      
      if (author != null)
        tasks = tasks.Where(t => Equals(t.Author, author))
          .ToList();
      
      if (businessUnit != null)
        tasks = tasks.Where(t => t.Assignee != null && t.Assignee.Department != null && t.Assignee.Department.BusinessUnit != null &&
                            Equals(t.Assignee.Department.BusinessUnit, businessUnit))
          .ToList();
      
      if (department != null)
        tasks = tasks.Where(t => t.Assignee != null && t.Assignee.Department != null &&
                            Equals(t.Assignee.Department, department))
          .ToList();
      
      if (performer != null)
        tasks = tasks.Where(t => Equals(t.Assignee, performer))
          .ToList();
      
      return tasks;
    }
    
    /// <summary>
    /// Получить сокращенные ФИО соисполнителей.
    /// </summary>
    /// <param name="task">Поручение.</param>
    /// <returns>Список сокращенных ФИО соисполнителей.</returns>
    private List<string> GetCoAssigneesShortNames(Sungero.RecordManagement.IActionItemExecutionTask task)
    {
      return task.CoAssignees.Select(ca => ca.Assignee.Person.ShortName).ToList();
    }
    
    /// <summary>
    /// Получить отчет выполнения задания
    /// </summary>
    /// <param name="task"></param>
    /// <returns></returns>
    private string GetReportText(Sungero.RecordManagement.IActionItemExecutionTask task)
    {
      var assignment = Sungero.RecordManagement.ActionItemExecutionAssignments.GetAll().Where(z => z.Task.Id == task.Id).FirstOrDefault();
      if(assignment != null) return assignment.ActiveText;
      else return string.Empty;
    }
    
    /// <summary>
    /// Сгенерировать PublicBody документа с отметкой об ЭП.
    /// </summary>
    /// <param name="document">Документ для преобразования.</param>
    /// <param name="versionId">Id версии, для генерации.</param>
    /// <param name="signatureMark">Отметка об ЭП (html).</param>
    /// <returns>Информация о результате генерации PublicBody для версии документа.</returns>
    [Public]
    public virtual void GeneratePublicBodyWithSignatureMark(Sungero.Docflow.IOfficialDocument document, int versionId, string signatureMark)
    {
      var version = document.Versions.SingleOrDefault(v => v.Id == versionId);
      System.IO.Stream pdfDocumentStream = null;
      using (var inputStream = new System.IO.MemoryStream())
      {
        version.Body.Read().CopyTo(inputStream);
        try
        {
          var pdfConverter = Sungero.AsposeExtensions.Converter.Create();
          var extension = version.BodyAssociatedApplication.Extension;
          pdfDocumentStream = pdfConverter.GeneratePdf(inputStream, extension);
          var htmlStampString = signatureMark;
          if (!string.IsNullOrEmpty(htmlStampString))
          {
            pdfDocumentStream = pdfConverter.AddSignatureMark(pdfDocumentStream, extension, htmlStampString, Sungero.Docflow.Resources.SignatureMarkAnchorSymbol, 5);
          }
        }
        catch (Exception e)
        {
          if (e is Sungero.AsposeExtensions.PdfConvertException)
            Logger.Error(Sungero.Docflow.Resources.PdfConvertErrorFormat(document.Id), e.InnerException);
          else
            Logger.Error(string.Format("{0} {1}", Sungero.Docflow.Resources.PdfConvertErrorFormat(document.Id), e.Message));
        }
      }
      version.PublicBody.Write(pdfDocumentStream);
      version.AssociatedApplication = Sungero.Content.AssociatedApplications.GetByExtension("pdf");
      pdfDocumentStream.Close();
    }
  }
}