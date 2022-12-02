using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using micros;
using System.IO;
using Newtonsoft.Json;
using System.Net;
using Newtonsoft.Json.Linq;
using RestSharp;
using RestSharp.Authenticators;
using micros.MultibankModule.MBKAssignment;
using micros.MultibankModule;

namespace micros.MultibankModule.Client
{
  partial class MBKAssignmentActions
  {
    public virtual void SendForFreeApproval(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      _obj.Save();
      
      var freeAprovalTask = Sungero.Docflow.FreeApprovalTasks.CreateAsSubtask(_obj.Task);
      freeAprovalTask.State.Properties.MaxDeadline.IsRequired = true;
      freeAprovalTask.ForApprovalGroup.ElectronicDocuments.Add(_obj.DocumentGroup.OfficialDocuments.FirstOrDefault());
      freeAprovalTask.ReceiveOnCompletion = Sungero.Docflow.FreeApprovalTask.ReceiveOnCompletion.Notice;
      freeAprovalTask.State.Properties.ReceiveOnCompletion.IsEnabled = false;
      freeAprovalTask.MaxDeadline = Calendar.Today.AddDays(1);
      freeAprovalTask.Show();
      _obj.Subtasks.Append(freeAprovalTask);
      e.CloseFormAfterAction = true;
    }

    public virtual bool CanSendForFreeApproval(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return true;
    }

    public virtual void SimpleFinish(Sungero.Workflow.Client.ExecuteResultActionArgs e)
    {
      
    }

    public virtual bool CanSimpleFinish(Sungero.Workflow.Client.CanExecuteResultActionArgs e)
    {
      return true;
    }

    public virtual void UpdateFromMultibank(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      var databook = _obj.MainTask.IntegrationDatabook;
      string jsonString = micros.MultibankModule.PublicFunctions.Module.Remote.GetMultibankJsonFromID(databook.Document).ToLower();
      micros.MultibankModule.PublicFunctions.Module.Remote.CreateDocument(Convert.ToBase64String(Encoding.Default.GetBytes(jsonString)));
      _obj.Save();
    }

    public virtual bool CanUpdateFromMultibank(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return true;
    }

    public virtual void Forward(Sungero.Workflow.Client.ExecuteResultActionArgs e)
    {
      var dialog = Dialogs.CreateInputDialog("Выбирите ответственного");
      var performer = dialog.AddSelect("Ответственный", true, Sungero.CoreEntities.Users.Null);
      bool action = false;
      if (dialog.Show() == DialogButtons.Ok)
      {
        _obj.Performer = performer.Value;
        _obj.Save();
        action = true;
      }
      if (!action) e.Cancel();
    }

    public virtual bool CanForward(Sungero.Workflow.Client.CanExecuteResultActionArgs e)
    {
      return true;
    }

    public virtual void Annul(Sungero.Workflow.Client.ExecuteResultActionArgs e)
    {
      if(_obj.DocumentStatus.Value.Value.Contains("Accept") ||
         _obj.DocumentStatus.Value.Value.Contains("Reject") ||
         _obj.DocumentStatus.Value.Value.Contains("Canceled"))
      {
        e.AddError("Документ уже был обработан на стороне сервиса обмена. Нажмите на кнопку завешнить без действия.");
        e.Cancel();
      }
      else
      {
        var databook = _obj.MainTask.IntegrationDatabook;
        bool action = false;
        if(databook != null && databook.Document.HasVersions && databook.Sign != null)
        {
          #region dialogCert
          var certificates = Certificates.GetAll().Where(x => x.Owner == Sungero.Company.Employees.Current);
          var dialogCert = Dialogs.CreateInputDialog("Выбор сертификата");
          var certificate = dialogCert.AddSelect("Выбор сертификата",true, certificates.FirstOrDefault()).From(certificates);
          dialogCert.Buttons.AddOkCancel();
          #endregion
          #region dialogPass
          var dialogPass = Dialogs.CreateInputDialog("Введите ваш пароль от DirectumRX");
          var password = dialogPass.AddPasswordString("Пароль", true);
          dialogPass.Buttons.AddOkCancel();
          #endregion
          if (dialogPass.Show() == DialogButtons.Ok)
          {
            string address = MultibankModule.PublicFunctions.Module.Remote.GetBuisnesUnitBoxForCompany(databook.Document.BusinessUnit).CurrenServerAddressmicros;
            string login = Sungero.Company.Employees.Current.Login.LoginName;
            
            var client = new RestClient(address + "integration/odata/MultibankModule");
            var request = new RestRequest("CheckPassword/", Method.GET);
            client.Authenticator = new HttpBasicAuthenticator(login, password.Value);
            IRestResponse response = client.Execute(request);
            if(response.StatusCode != HttpStatusCode.OK) Dialogs.ShowMessage("Ошибка, пожалуйста проверьте корректность пароля", MessageType.Error);
            else
            {
              if (dialogCert.Show() == DialogButtons.Ok)
              {
                string pass = password.Value;
                if (pass.Contains(@"\")) pass = pass.Replace(@"\", @"\\");
                if (login.Contains(@"\")) login = login.Replace(@"\", @"\\");
                Dictionary<string, byte[]> signdata = new Dictionary<string, byte[]>();
                string forSign = "forsign: {\"address\": \"{serverAddress}\", \"login\": \"{login}\", \"password\": \"{password}\", \"document_id\": {document_id}, \"issigned\": {issigned}, \"pkcs7\": \"{pkcs7}\"}";
                
                string stringForSign = MultibankModule.PublicFunctions.Module.Remote.GNKString(databook, "cancel", string.Empty);
                string stringForSign64 = Convert.ToBase64String(Encoding.Default.GetBytes(stringForSign));

                forSign = forSign.Replace("{serverAddress}", address).Replace("{login}", login).Replace("{password}", pass).Replace("{document_id}", _obj.DocumentGroup.OfficialDocuments.FirstOrDefault().Id.ToString()).Replace("{issigned}", "false").Replace("{pkcs7}", stringForSign64);
                Logger.Debug("String for sign: " + forSign);
                signdata.Add("1", Encoding.Default.GetBytes(forSign));
                var externalSign = ExternalSignatures.Sign(certificate.Value, signdata);
                var signedString = Convert.ToBase64String(externalSign.FirstOrDefault().Value);
                string status = MultibankModule.PublicFunctions.Module.Remote.CancelDocumentInMultibank(databook);
                var document = databook.Document;
                if (!status.Contains("\"success\":true"))
                {
                  document.VerificationState = document.VerificationStateAllowedItems.Where(x => x.Value.Contains("Error")).FirstOrDefault();
                  Dialogs.ShowMessage("", MultibankModule.PublicFunctions.Module.Remote.GetMessageFromResponse(status), MessageType.Error);
                }
                else
                {
                  document.VerificationState = document.VerificationStateAllowedItems.Where(x => x.Value.Contains("Completed")).FirstOrDefault();
                  action = true;
                  _obj.DocumentStatus = MultibankModule.MBKAssignment.DocumentStatus.Finished;
                }
              }
            }
          }
        }
        if (!action) e.Cancel();
      }
    }

    public virtual bool CanAnnul(Sungero.Workflow.Client.CanExecuteResultActionArgs e)
    {
      return true;
    }

    public virtual void Reject(Sungero.Workflow.Client.ExecuteResultActionArgs e)
    {
      if(_obj.DocumentStatus.Value.Value.Contains("Accept") ||
         _obj.DocumentStatus.Value.Value.Contains("Reject") ||
         _obj.DocumentStatus.Value.Value.Contains("Canceled"))
      {
        e.AddError("Документ уже был обработан на стороне сервиса обмена. Нажмите на кнопку завешнить без действия.");
        e.Cancel();
      }
      else
      {
        var databook = _obj.MainTask.IntegrationDatabook;
        bool action = false;
        if(databook != null && databook.Document.HasVersions && databook.Sign != null)
        {
          #region dialogCert
          var certificates = Certificates.GetAll().Where(x => x.Owner == Sungero.Company.Employees.Current);
          var dialogCert = Dialogs.CreateInputDialog("Выбор сертификата");
          var certificate = dialogCert.AddSelect("Выбор сертификата",true, certificates.FirstOrDefault()).From(certificates);
          dialogCert.Buttons.AddOkCancel();
          dialogCert.SetOnButtonClick(h => {if (h.Button == DialogButtons.Cancel) e.Cancel();});
          #endregion
          #region dialogPass
          var dialogPass = Dialogs.CreateInputDialog("Введите ваш пароль от DirectumRX");
          var password = dialogPass.AddPasswordString("Пароль", true);
          dialogPass.Buttons.AddOkCancel();
          dialogPass.SetOnButtonClick(h => {if (h.Button == DialogButtons.Cancel) e.Cancel();});
          #endregion
          #region diaglogOPTS
          var dialogOPTS = Dialogs.CreateInputDialog("Отклонить");
          var opts = dialogOPTS.AddString("Введите причину отклонения", true);
          dialogOPTS.Buttons.AddOkCancel();
          dialogOPTS.SetOnButtonClick(h => {if (h.Button == DialogButtons.Cancel) e.Cancel();});
          #endregion
          if (dialogPass.Show() == DialogButtons.Ok)
          {
            string address = MultibankModule.PublicFunctions.Module.Remote.GetBuisnesUnitBoxForCompany(databook.Document.BusinessUnit).CurrenServerAddressmicros;
            string login = Sungero.Company.Employees.Current.Login.LoginName;
            
            var client = new RestClient(address + "integration/odata/MultibankModule");
            var request = new RestRequest("CheckPassword/", Method.GET);
            client.Authenticator = new HttpBasicAuthenticator(login, password.Value);
            IRestResponse response = client.Execute(request);
            if(response.StatusCode != HttpStatusCode.OK) Dialogs.ShowMessage("Ошибка, пожалуйста проверьте корректность пароля", MessageType.Error);
            else
            {
              if (dialogOPTS.Show() == DialogButtons.Ok)
              {
                if (dialogCert.Show() == DialogButtons.Ok)
                {
                  string pass = password.Value;
                  if (pass.Contains(@"\")) pass = pass.Replace(@"\", @"\\");
                  if (login.Contains(@"\")) login = login.Replace(@"\", @"\\");
                  Dictionary<string, byte[]> signdata = new Dictionary<string, byte[]>();
                  string forSign = "forsign: {\"address\": \"{serverAddress}\", \"login\": \"{login}\", \"password\": \"{password}\", \"document_id\": {document_id}, \"issigned\": {issigned}, \"pkcs7\": \"{pkcs7}\"}";
                  
                  string stringForSign = MultibankModule.PublicFunctions.Module.Remote.GNKString(databook, "reject", opts.Value);
                  string stringForSign64 = Convert.ToBase64String(Encoding.Default.GetBytes(stringForSign));
                  forSign = forSign.Replace("{serverAddress}", address).Replace("{login}", login).Replace("{password}", pass).Replace("{document_id}", _obj.DocumentGroup.OfficialDocuments.FirstOrDefault().Id.ToString()).Replace("{issigned}", "false").Replace("{pkcs7}", stringForSign64);
                  Logger.Debug("String for sign: " + forSign);
                  signdata.Add("1", Encoding.Default.GetBytes(forSign));
                  var externalSign = ExternalSignatures.Sign(certificate.Value, signdata);
                  var signedString = Convert.ToBase64String(externalSign.FirstOrDefault().Value);
                  string status = MultibankModule.PublicFunctions.Module.Remote.RejectDocumentInMultibank(databook, opts.Value);
                  var document = databook.Document;
                  if (!status.Contains("\"success\":true"))
                  {
                    document.VerificationState = document.VerificationStateAllowedItems.Where(x => x.Value.Contains("Error")).FirstOrDefault();
                    Dialogs.ShowMessage("", MultibankModule.PublicFunctions.Module.Remote.GetMessageFromResponse(status), MessageType.Error);
                  }
                  else
                  {
                    document.VerificationState = document.VerificationStateAllowedItems.Where(x => x.Value.Contains("Completed")).FirstOrDefault();
                    action = true;
                    _obj.DocumentStatus = MultibankModule.MBKAssignment.DocumentStatus.Finished;
                  }
                }
              }
            }
          }
        }
        if (!action) e.Cancel();
      }
    }

    public virtual bool CanReject(Sungero.Workflow.Client.CanExecuteResultActionArgs e)
    {
      return true;
    }

    public virtual void Accept(Sungero.Workflow.Client.ExecuteResultActionArgs e)
    {
      if(_obj.DocumentStatus.Value.Value.Contains("Accept") ||
         _obj.DocumentStatus.Value.Value.Contains("Reject") ||
         _obj.DocumentStatus.Value.Value.Contains("Canceled"))
      {
        e.AddError("Документ уже был обработан на стороне сервиса обмена. Нажмите на кнопку завешнить без действия.");
        e.Cancel();
      }
      else
      {
        var databook = _obj.MainTask.IntegrationDatabook;
        bool action = false;
        if(databook != null && databook.Document.HasVersions && databook.Sign != null)
        {
          #region dialogCert
          var certificates = Certificates.GetAll().Where(x => x.Owner == Sungero.Company.Employees.Current);
          var dialogCert = Dialogs.CreateInputDialog("Выбор сертификата");
          var certificate = dialogCert.AddSelect("Выбор сертификата",true, certificates.FirstOrDefault()).From(certificates);
          dialogCert.Buttons.AddOkCancel();
          dialogCert.SetOnButtonClick(h => {if (h.Button == DialogButtons.Cancel) e.Cancel();});
          #endregion
          #region dialogPass
          var dialogPass = Dialogs.CreateInputDialog("Введите ваш пароль от DirectumRX");
          var password = dialogPass.AddPasswordString("Пароль", true);
          dialogPass.Buttons.AddOkCancel();
          dialogPass.SetOnButtonClick(h => {if (h.Button == DialogButtons.Cancel) e.Cancel();});
          #endregion
          if (dialogPass.Show() == DialogButtons.Ok)
          {
            string address = MultibankModule.PublicFunctions.Module.Remote.GetBuisnesUnitBoxForCompany(databook.Document.BusinessUnit).CurrenServerAddressmicros;
            string login = Sungero.Company.Employees.Current.Login.LoginName;
            
            var client = new RestClient(address + "integration/odata/MultibankModule");
            var request = new RestRequest("CheckPassword/", Method.GET);
            client.Authenticator = new HttpBasicAuthenticator(login, password.Value);
            IRestResponse response = client.Execute(request);
            if(response.StatusCode != HttpStatusCode.OK) Dialogs.ShowMessage("Ошибка, пожалуйста проверьте корректность пароля", MessageType.Error);
            else
            {
              if (dialogCert.Show() == DialogButtons.Ok)
              {
                string pass = password.Value;
                if (pass.Contains(@"\")) pass = pass.Replace(@"\", @"\\");
                if (login.Contains(@"\")) login = login.Replace(@"\", @"\\");
                Dictionary<string, byte[]> signdata = new Dictionary<string, byte[]>();
                string forSign = "forsign: {\"address\": \"{serverAddress}\", \"login\": \"{login}\", \"password\": \"{password}\", \"document_id\": {document_id}, \"issigned\": {issigned}, \"pkcs7\": \"{pkcs7}\"}";
                
                string stringForSign = micros.MultibankModule.PublicFunctions.Module.Remote.GNKString(databook, "accept", String.Empty);
                forSign = forSign.Replace("{serverAddress}", address).Replace("{login}", login).Replace("{password}", pass).Replace("{document_id}", _obj.DocumentGroup.OfficialDocuments.FirstOrDefault().Id.ToString()).Replace("{issigned}", "true").Replace("{pkcs7}", stringForSign);
                Logger.Debug("String for sign: " + forSign.Replace(password.Value, "******"));
                signdata.Add("1", Encoding.Default.GetBytes(forSign));
                var externalSign = ExternalSignatures.Sign(certificate.Value, signdata);
                //var signedString = Convert.ToBase64String(externalSign.FirstOrDefault().Value);
                string status = MultibankModule.PublicFunctions.Module.Remote.ReturnSigningDocumentInMultibank(databook);
                var document = databook.Document;
                if (!status.Contains("\"success\":true"))
                {
                  document.VerificationState = document.VerificationStateAllowedItems.Where(x => x.Value.Contains("Error")).FirstOrDefault();
                  Dialogs.ShowMessage("", MultibankModule.PublicFunctions.Module.Remote.GetMessageFromResponse(status), MessageType.Error);
                }
                else
                {
                  document.VerificationState = document.VerificationStateAllowedItems.Where(x => x.Value.Contains("Completed")).FirstOrDefault();
                  _obj.DocumentStatus = MultibankModule.MBKAssignment.DocumentStatus.Finished;
                  action = true;
                }
              }
            }
          }
        }
        if (!action) e.Cancel();
      }
    }

    public virtual bool CanAccept(Sungero.Workflow.Client.CanExecuteResultActionArgs e)
    {
      return true;
    }
  }
}