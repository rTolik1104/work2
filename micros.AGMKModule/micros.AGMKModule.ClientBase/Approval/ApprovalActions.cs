using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using micros.AGMKModule.Approval;

namespace micros.AGMKModule.Client
{
  partial class ApprovalActions
  {

    public virtual void ForRevision(Sungero.Workflow.Client.ExecuteResultActionArgs e)
    {
      // Валидация заполненности активного текста.
      if (_obj.ActiveText == null)
      {
        e.AddError("Заполните текст доработки");
        e.Cancel();
      }
      
    }

    public virtual bool CanForRevision(Sungero.Workflow.Client.CanExecuteResultActionArgs e)
    {
      return true;
    }

    public virtual void Approved(Sungero.Workflow.Client.ExecuteResultActionArgs e)
    {
      
      var document = _obj.DocumentGroup.OfficialDocuments.First();
      if (!document.HasVersions)
      {
        e.AddError("Создайте версию документа");
        e.Cancel();
      }

      var accessRightsGranted = Sungero.Docflow.PublicFunctions.Module.ShowDialogGrantAccessRights(_obj, _obj.OtherGroup.All.ToList());
      if (accessRightsGranted == false)
        e.Cancel();
      
      var confirmationMessage = e.Action.ConfirmationMessage;
      if (_obj.AddendaGroup.OfficialDocuments.Any())
        confirmationMessage = Sungero.Docflow.ApprovalAssignments.Resources.ApprovalConfirmationMessage;
      //if (accessRightsGranted == null && !Sungero.Docflow.ApprovalTask.ConfirmCompleteAssignment(document, confirmationMessage, Constants.ApprovalTask.ApprovalAssignmentConfirmDialogID.Approved, false))
      //e.Cancel();
      
      Signatures.Endorse(document.LastVersion, null, "");
    }

    public virtual bool CanApproved(Sungero.Workflow.Client.CanExecuteResultActionArgs e)
    {
      return true;
    }

  }

}