using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using micros.AGMKModule.SimpleReview;

namespace micros.AGMKModule.Client
{
  partial class SimpleReviewActions
  {
    public virtual void Informed(Sungero.Workflow.Client.ExecuteResultActionArgs e)
    {
      
    }

    public virtual bool CanInformed(Sungero.Workflow.Client.CanExecuteResultActionArgs e)
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
      var assignedBy = micros.AGMKModule.MemoTasks.As(_obj.Task).Addressee;
      foreach (var otherGroupAttachment in _obj.OtherGroup.All)
        task.OtherGroup.All.Add(otherGroupAttachment);
      task.ShowModal();
      task.AssignedBy = Sungero.Company.Employees.Get(_obj.Performer.Id);
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

    public virtual void ForExecution(Sungero.Workflow.Client.ExecuteResultActionArgs e)
    {
      if (!Functions.MemoTask.HasDocumentAndCanRead(MemoTasks.As(_obj.Task)))
      {
        e.AddError(Sungero.RecordManagement.DocumentReviewTasks.Resources.NoRightsToDocument);
        e.Cancel();
      }
      
      // В качестве проектов резолюции нельзя отправить поручения-непроекты.
      //if (_obj.ResolutionGroup.ActionItemExecutionTasks.Where(d => d.Status != Sungero.RecordManagement.ActionItemExecutionAssignment.Status.InProcess).Any(a => a.IsDraftResolution != true))
      //{
      //  e.AddError(Sungero.RecordManagement.DocumentReviewTasks.Resources.FindNotDraftResolution);
      //  e.Cancel();
      //}
      
      Functions.MemoTask.Remote.GiveRightsToAttachments(MemoTasks.As(_obj.Task));
      
      Functions.MemoTask.CheckOverdueActionItemExecutionTasks(MemoTasks.As(_obj.Task), e);
      
      if (_obj.ResolutionGroup.ActionItemExecutionTasks.Any(x => x.AssignedBy == _obj.Performer))
        micros.AGMKModule.PublicFunctions.MemoTask.StartActionItemsForDraftResolutionSimple(MemoTasks.As(_obj.Task), _obj);
      else
        e.AddError("В задаче не поручений от вашего имени");
    }

    public virtual bool CanForExecution(Sungero.Workflow.Client.CanExecuteResultActionArgs e)
    {
      return true;
    }

  }


}