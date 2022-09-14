using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using micros.AGMKModule.Resolution;

namespace micros.AGMKModule.Client
{
  partial class ResolutionActions
  {
    public virtual void PrintResolution(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      _obj.Save();
      var report = Sungero.RecordManagement.Reports.GetDraftResolutionReport();
      var actionItems = _obj.ResolutionGroup.ActionItemExecutionTasks;
      report.Resolution.AddRange(actionItems);
      report.TextResolution = _obj.ActiveText;
      report.Document = _obj.DocumentGroup.OfficialDocuments.FirstOrDefault();
      report.Author = MemoTasks.As(_obj.Task).Addressee;
      report.Open();
    }

    public virtual bool CanPrintResolution(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return true;
    }

    public virtual void AddResolution(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      _obj.Save();
      
      var document = _obj.DocumentGroup.OfficialDocuments.FirstOrDefault();
      var task = Sungero.RecordManagement.PublicFunctions.Module.Remote.CreateActionItemExecution(document);
      var assignee = task.Assignee ?? Users.Current;
      task.MaxDeadline = _obj.Deadline.HasValue ? _obj.Deadline.Value : Calendar.Today.AddWorkingDays(assignee, 2);
      task.IsDraftResolution = true;
      var assignedBy = Sungero.Company.ManagersAssistants.GetAll().Where(a => a.Assistant == _obj.Performer).FirstOrDefault().Manager;
      task.AssignedBy = Sungero.Docflow.PublicFunctions.Module.Remote.IsUsersCanBeResolutionAuthor(document, assignedBy) ? assignedBy : null;
      foreach (var otherGroupAttachment in _obj.OtherGroup.All)
        task.OtherGroup.All.Add(otherGroupAttachment);
      task.ShowModal();
      if (!task.State.IsInserted)
      {
        _obj.ResolutionGroup.ActionItemExecutionTasks.Add(task);
        _obj.Save();
      }
    }

    public virtual bool CanAddResolution(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return true;
    }

    public virtual void Complete(Sungero.Workflow.Client.ExecuteResultActionArgs e)
    {
      
    }

    public virtual bool CanComplete(Sungero.Workflow.Client.CanExecuteResultActionArgs e)
    {
      return true;
    }

  }

}