using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Company;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.RecordManagement.ActionItemExecutionTask;

namespace micros.AGMKModule
{
  partial class MeetingsReportServerHandlers
  {

    public override void AfterExecute(Sungero.Reporting.Server.AfterExecuteEventArgs e)
    {
      // Удалить временные данные из таблицы.
      Sungero.Docflow.PublicFunctions.Module.DeleteReportData(Constants.MeetingsReport.SourceTableName, MeetingsReport.ReportSessionId);
    }

    public override void BeforeExecute(Sungero.Reporting.Server.BeforeExecuteEventArgs e)
    {
      #region Параметры и дата выполнения отчета
      
      MeetingsReport.ReportSessionId = Guid.NewGuid().ToString();
      MeetingsReport.ReportDate = Calendar.Now;
      
      if (string.IsNullOrEmpty(MeetingsReport.Header))
      {
        if (MeetingsReport.Meeting == null && MeetingsReport.IsMeetingsCoverContext == true)
          MeetingsReport.Header = Sungero.RecordManagement.Resources.ActionItemsExecutionReportForMeetings;
        else
          MeetingsReport.Header = Sungero.RecordManagement.Resources.ActionItemsExecutionReport;
      }
      
      if (MeetingsReport.Meeting != null)
      {
        MeetingsReport.Subheader = Sungero.RecordManagement.Reports.Resources.ActionItemsExecutionReport.HeaderMeetingFormat(MeetingsReport.Meeting.Name);
        
        if (MeetingsReport.IsMeetingsCoverContext == true)
        {
          var deadlineHeaderString = Sungero.RecordManagement.Reports.Resources.ActionItemsExecutionReport.HeaderDeadlineFormat(MeetingsReport.BeginDate.Value.ToShortDateString(),
                                                                                                       MeetingsReport.ClientEndDate.Value.ToShortDateString());
          MeetingsReport.Subheader += string.Format("\n{0}", deadlineHeaderString);
        }
      }
      else if (MeetingsReport.Document == null)
        MeetingsReport.Subheader = Sungero.RecordManagement.Reports.Resources.ActionItemsExecutionReport.HeaderDeadlineFormat(MeetingsReport.BeginDate.Value.ToShortDateString(),
                                                                                                                 MeetingsReport.ClientEndDate.Value.ToShortDateString());
      else
        MeetingsReport.Subheader = Sungero.RecordManagement.Reports.Resources.ActionItemsExecutionReport.HeaderDocumentFormat(MeetingsReport.Document.Name);
      
      if (MeetingsReport.Author != null)
        MeetingsReport.ParamsDescriprion += Sungero.RecordManagement.Reports.Resources.ActionItemsExecutionReport.FilterAuthorFormat(MeetingsReport.Author.Person.ShortName,
                                                                                                                        System.Environment.NewLine);
      
      if (MeetingsReport.BusinessUnit != null)
        MeetingsReport.ParamsDescriprion += Sungero.RecordManagement.Reports.Resources.ActionItemsExecutionReport.FilterBusinessUnitFormat(MeetingsReport.BusinessUnit.Name,
                                                                                                                              System.Environment.NewLine);
      
      if (MeetingsReport.Department != null)
        MeetingsReport.ParamsDescriprion += Sungero.RecordManagement.Reports.Resources.ActionItemsExecutionReport.FilterDepartmentFormat(MeetingsReport.Department.Name,
                                                                                                                            System.Environment.NewLine);
      
      if (MeetingsReport.Performer != null)
      {
        var performerName = Employees.Is(MeetingsReport.Performer) ?
          Employees.As(MeetingsReport.Performer).Person.ShortName :
          MeetingsReport.Performer.Name;
        MeetingsReport.ParamsDescriprion += Sungero.RecordManagement.Reports.Resources.ActionItemsExecutionReport.FilterResponsibleFormat(performerName,
                                                                                                                             System.Environment.NewLine);
      }
      
      #endregion
      
      #region Расчет итогов
      var actionItems = Functions.Module.GetActionItemCompletionData(MeetingsReport.Meeting,
                                                                     MeetingsReport.Document,
                                                                     MeetingsReport.BeginDate,
                                                                     MeetingsReport.EndDate,
                                                                     MeetingsReport.Author,
                                                                     MeetingsReport.BusinessUnit,
                                                                     MeetingsReport.Department,
                                                                     MeetingsReport.Performer,
                                                                     MeetingsReport.DocumentType,
                                                                     MeetingsReport.IsMeetingsCoverContext,
                                                                     true);
      MeetingsReport.TotalCount = actionItems.Count();
      MeetingsReport.Completed = actionItems.Where(j => j.Status == Sungero.Workflow.Task.Status.Completed).Count();
      MeetingsReport.CompletedInTime = actionItems
        .Where(j => j.Status == Sungero.Workflow.Task.Status.Completed)
        .Where(j => Sungero.Docflow.PublicFunctions.Module.CalculateDelay(j.Deadline, j.ActualDate.Value, j.Assignee) == 0).Count();
      MeetingsReport.CompletedOverdue = actionItems
        .Where(j => j.Status == Sungero.Workflow.Task.Status.Completed)
        .Where(j => Sungero.Docflow.PublicFunctions.Module.CalculateDelay(j.Deadline, j.ActualDate.Value, j.Assignee) > 0).Count();
      MeetingsReport.InProcess = actionItems.Where(j => j.Status == Sungero.Workflow.Task.Status.InProcess).Count();
      
      MeetingsReport.InProcessOverdue = actionItems
        .Where(j => j.Status == Sungero.Workflow.Task.Status.InProcess)
        .Where(j => Sungero.Docflow.PublicFunctions.Module.CalculateDelay(j.Deadline, Calendar.Now, j.Assignee) > 0).Count();
      
      if (MeetingsReport.TotalCount != 0)
      {
        var inTimeActionItems = MeetingsReport.CompletedInTime + MeetingsReport.InProcess - MeetingsReport.InProcessOverdue;
        MeetingsReport.ExecutiveDisciplineLevel =
          string.Format("{0:P2}", inTimeActionItems / (double)MeetingsReport.TotalCount);
      }
      else
        MeetingsReport.ExecutiveDisciplineLevel = Sungero.RecordManagement.Reports.Resources.ActionItemsExecutionReport.NoAnyActionItems;
      
      #endregion
      
      var dataTable = new List<Structures.MeetingsReport.TableLine>();
      foreach (var actionItem in actionItems.OrderBy(a => a.Deadline))
      {
        var tableLine = Structures.MeetingsReport.TableLine.Create();
        
        // ИД и ссылка.
        tableLine.Id = actionItem.Id;
        tableLine.Hyperlink = Sungero.Core.Hyperlinks.Get(Sungero.RecordManagement.ActionItemExecutionTasks.Info, actionItem.Id);
        
        // Поручение.
        tableLine.ActionItemText = actionItem.ActionItem;
        
        // Автор.
        var author = Employees.As(actionItem.Author);
        if (author != null && author.Person != null)
          tableLine.Author = author.Person.ShortName;
        else
          tableLine.Author = actionItem.Author.Name;
        
        // TODO Lomagin: Убрать замену на пробел после исправления 33360. Сделано в рамках бага 33343.
        // Сделано для корректного переноса инициалов, если фамилия длинная.
        tableLine.Author = tableLine.Author.Replace("\u00A0", " ");
        
        // Статус.
        tableLine.State = string.Empty;
        if (actionItem.ExecutionState != null)
          tableLine.State = Sungero.RecordManagement.ActionItemExecutionTasks.Info.Properties.ExecutionState.GetLocalizedValue(actionItem.ExecutionState.Value);
        
        // Даты и просрочка.
        tableLine.PlanDate = string.Empty;
        if (actionItem.Deadline.HasValue)
        {
          var deadline = Calendar.ToUserTime(actionItem.Deadline.Value);
          tableLine.PlanDate = Sungero.Docflow.PublicFunctions.Module.ToShortDateShortTime(deadline);
          tableLine.PlanDateSort = actionItem.Deadline.Value;
        }
        tableLine.ActualDate = string.Empty;
        var isCompleted = actionItem.Status == Sungero.Workflow.Task.Status.Completed;
        if (isCompleted)
        {
          var endDate = actionItem.ActualDate.HasValue ? actionItem.ActualDate.Value : Calendar.Now;
          tableLine.ActualDate = Sungero.Docflow.PublicFunctions.Module.ToShortDateShortTime(Calendar.ToUserTime(endDate));
          tableLine.Overdue = Sungero.Docflow.PublicFunctions.Module.CalculateDelay(actionItem.Deadline, endDate, actionItem.Assignee);
        }
        else
          tableLine.Overdue = Sungero.Docflow.PublicFunctions.Module.CalculateDelay(actionItem.Deadline, Calendar.Now, actionItem.Assignee);
        
        // Исполнители.
        tableLine.Assignee = actionItem.Assignee.Person.ShortName;
        
        tableLine.CoAssignees = string.Join(", ", actionItem.CoAssigneesShortNames);
        
        tableLine.Texts = actionItem.ActiveText;
        
        tableLine.ReportSessionId = MeetingsReport.ReportSessionId;
        
        dataTable.Add(tableLine);
      }
      
      var dataTableSort = dataTable.Where(d => !string.IsNullOrEmpty(d.PlanDate)).OrderBy(d => d.PlanDateSort).ToList();
      dataTableSort.AddRange(dataTable.Where(d => string.IsNullOrEmpty(d.PlanDate)).OrderBy(d => d.Id).ToList());
      var rowIndex = 1;
      foreach (var item in dataTableSort)
      {
        item.RowIndex = rowIndex++;
      }
      
      Sungero.Docflow.PublicFunctions.Module.WriteStructuresToTable(Constants.MeetingsReport.SourceTableName, dataTableSort);
      
      using (var command = SQL.GetCurrentConnection().CreateCommand())
      {
        // Заполнить таблицу именами документов.
        command.CommandText = string.Format(Queries.MeetingsReport.PasteDocumentNames, Constants.MeetingsReport.SourceTableName, MeetingsReport.ReportSessionId);
        command.ExecuteNonQuery();
      }
    }

  }
}