using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using micros.MultibankSolution.ApprovalSigningAssignment;

namespace micros.MultibankSolution.Client
{
  partial class ApprovalSigningAssignmentActions
  {
    public override void CreateSubtask(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      base.CreateSubtask(e);
    }

    public override bool CanCreateSubtask(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return base.CanCreateSubtask(e);
    }

    public virtual void AssignInMultibankmicros(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      if (!micros.MultibankModule.PublicFunctions.Module.Remote.IsSignedInMultibank(_obj.DocumentGroup.OfficialDocuments.Single()))
        Dialogs.ShowMessage("Документ уже подписан");
      else
      {
        string url = micros.MultibankModule.Resources.OpenInvoicePage;
        string json = micros.MultibankModule.PublicFunctions.Module.Remote.GetJsonFromBody(_obj.DocumentGroup.OfficialDocuments.Single());
        string document_id = micros.MultibankModule.PublicFunctions.Module.Remote.GetJsonMultibankGuid(json);
        url = url.Replace("{document_id}", document_id);
        Hyperlinks.Open(url);
      }
    }

    public virtual bool CanAssignInMultibankmicros(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return true;
    }
  }

}