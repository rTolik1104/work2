using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using micros.AGMKSolution.Memo;

namespace micros.AGMKSolution.Client
{
  partial class MemoActions
  {
    public virtual void SendToReviewmicros(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      var registrationState = _obj.RegistrationState;
      if (registrationState == null || (Enumeration)registrationState != Sungero.Docflow.OfficialDocument.RegistrationState.Registered)
      {
        e.AddError(Sungero.Docflow.ApprovalTasks.Resources.ToPerformNeedRegisterDocument);
      }
      else
      {
        var task = AGMKModule.MemoTasks.Create();
        if (_obj.IsManyAddressees.Value == true)
        {
          foreach (var addressee in _obj.Addressees)
          {
            task.Addressees.AddNew().Addressee = addressee.Addressee;
          }
        }
        else task.Addressee = _obj.Addressee;
        task.DocumentGroup.OfficialDocuments.Add(_obj);
        task.Signatory = _obj.OurSignatory;
        task.Author = _obj.Author;
        task.Subject = "Прошу согласовать: " + _obj.Name;
        task.ReqApprovers.AddNew().Approver = Sungero.Company.Employees.GetAll().Where(x => x.Id == _obj.Author.Id).FirstOrDefault().Department.Manager;
        _obj.Save();
        task.Show();
      }
    }

    public virtual bool CanSendToReviewmicros(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return true;
    }

  }

}