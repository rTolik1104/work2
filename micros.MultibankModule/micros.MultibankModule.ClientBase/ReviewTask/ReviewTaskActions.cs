using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using micros.MultibankModule.ReviewTask;

namespace micros.MultibankModule.Client
{
  partial class ReviewTaskActions
  {
    public virtual void SendForFreeApproval(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      _obj.Save();
      
      var freeAprovalTask = Sungero.Docflow.PublicFunctions.Module.Remote.CreateFreeApprovalTask(_obj.DocumentGroup.OfficialDocuments.FirstOrDefault());
      freeAprovalTask.Show();
      _obj.Subtasks.Append(freeAprovalTask);
      e.CloseFormAfterAction = true;
    }

    public virtual bool CanSendForFreeApproval(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return true;
    }

  }

}