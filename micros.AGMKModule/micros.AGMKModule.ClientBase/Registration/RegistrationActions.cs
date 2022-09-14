using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using micros.AGMKModule.Registration;

namespace micros.AGMKModule.Client
{
  partial class RegistrationActions
  {
    public virtual void ForRevision(Sungero.Workflow.Client.ExecuteResultActionArgs e)
    {
      // Проверить заполненность активного текста.
      if (string.IsNullOrWhiteSpace(_obj.ActiveText))
      {
        e.AddError(Sungero.Docflow.ApprovalTasks.Resources.NeedTextForRework);
        e.Cancel();
      }
    }

    public virtual bool CanForRevision(Sungero.Workflow.Client.CanExecuteResultActionArgs e)
    {
      return true;
    }

    public virtual void Complete(Sungero.Workflow.Client.ExecuteResultActionArgs e)
    {
      // Проверить зарегистрированность документа.
      var document = _obj.DocumentGroup.OfficialDocuments.First();
      var registrationState = document.RegistrationState;
      if (registrationState == null || (Enumeration)registrationState != Sungero.Docflow.OfficialDocument.RegistrationState.Registered)
      {
        e.AddError(Sungero.Docflow.ApprovalTasks.Resources.ToPerformNeedRegisterDocument);
        e.Cancel();
      }
    }

    public virtual bool CanComplete(Sungero.Workflow.Client.CanExecuteResultActionArgs e)
    {
      return true;
    }

  }

}