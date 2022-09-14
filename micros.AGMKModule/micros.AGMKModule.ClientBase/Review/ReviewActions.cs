using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using micros.AGMKModule.Review;

namespace micros.AGMKModule.Client
{
  partial class ReviewActions
  {
    public virtual void ForRevision(Sungero.Workflow.Client.ExecuteResultActionArgs e)
    {
      
    }

    public virtual bool CanForRevision(Sungero.Workflow.Client.CanExecuteResultActionArgs e)
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
      
      //В качестве проектов резолюции нельзя отправить поручения-непроекты.
      if (_obj.ResolutionGroup.ActionItemExecutionTasks.Where(d => d.Status != Sungero.RecordManagement.ActionItemExecutionAssignment.Status.InProcess).Any(a => a.IsDraftResolution != true))
      {
        e.AddError(Sungero.RecordManagement.DocumentReviewTasks.Resources.FindNotDraftResolution);
        e.Cancel();
      }
      
      Functions.MemoTask.CheckOverdueActionItemExecutionTasks(MemoTasks.As(_obj.Task), e);
      
      micros.AGMKModule.PublicFunctions.MemoTask.StartActionItemsForDraftResolution(MemoTasks.As(_obj.Task), _obj);
    }

    public virtual bool CanForExecution(Sungero.Workflow.Client.CanExecuteResultActionArgs e)
    {
      return true;
    }

  }


}