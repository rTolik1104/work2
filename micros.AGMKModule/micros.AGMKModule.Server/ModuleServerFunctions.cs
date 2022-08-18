using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace micros.AGMKModule.Server
{
  public class ModuleFunctions
  {
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
    public virtual List<Structures.Module.LightActiomItem> GetActionItemCompletionData(Sungero.Meetings.IMeeting meeting,
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
          
          if (documentType != null)
          {
            var documents = Sungero.Docflow.OfficialDocuments.GetAll(d => d.DocumentKind.DocumentType == documentType);
            
            // В Hibernate обращаться к группам вложений задачи можно только через метаданные.
            query = query.Where(t => t.AttachmentDetails.Any(g => g.GroupId == documentsGroupGuid && documents.Any(m => m.Id == g.AttachmentId)));
          }
          
          if (meeting != null && isMeetingsCoverContext != true)
          {
            var minutesList = Sungero.Meetings.Minuteses.GetAll(d => Equals(d.Meeting, meeting));
            
            // В Hibernate обращаться к группам вложений задачи можно только через метаданные.
            query = query.Where(t => t.AttachmentDetails.Any(g => g.GroupId == documentsGroupGuid && minutesList.Any(m => m.Id == g.AttachmentId)));
          }
          else if (document != null)
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
          
          if (isMeetingsCoverContext == true)
          {
            var minutesList = meeting == null ?
              Sungero.Meetings.Minuteses.GetAll(d => d.Meeting != null) :
              Sungero.Meetings.Minuteses.GetAll(d => Equals(d.Meeting, meeting));
            
            query = query.Where(t => t.AttachmentDetails.Any(g => g.GroupId == documentsGroupGuid && minutesList.Any(m => m.Id == g.AttachmentId)));
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
  }
}