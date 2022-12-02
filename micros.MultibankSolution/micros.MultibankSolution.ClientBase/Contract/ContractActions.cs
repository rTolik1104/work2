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
using micros.MultibankSolution.Contract;

namespace micros.MultibankSolution.Client
{
  partial class ContractActions
  {
    public virtual void OpenInMultibankmicros(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      string url = micros.MultibankModule.PublicFunctions.Module.Remote.GetBuisnesUnitBoxForCompany(_obj.BusinessUnit).ExchangeService.InvoiceURLmicros;
      string document_id = micros.MultibankModule.IntegrationDatabooks.GetAll().Where(x => x.Document == _obj).FirstOrDefault().Guid;
      url = url.Replace("{document_id}", document_id);
      Hyperlinks.Open(url);
    }

    public virtual bool CanOpenInMultibankmicros(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return true;
    }

    public virtual void UpdateFromMultibankmicros(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      var databook = MultibankModule.IntegrationDatabooks.GetAll().Where(x => x.Document == _obj).FirstOrDefault();
      string jsonString = micros.MultibankModule.PublicFunctions.Module.Remote.GetMultibankJsonFromID(_obj).ToLower();
      micros.MultibankModule.PublicFunctions.Module.Remote.UpdateContractFromJson(jsonString, _obj, databook.IsIncoming.Value);
      _obj.Save();
    }

    public virtual bool CanUpdateFromMultibankmicros(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return true;
    }

    public virtual void Rejectmicros(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      var databook = micros.MultibankModule.IntegrationDatabooks.GetAll().Where(x => x.Document.Id == e.Entity.Id).FirstOrDefault();
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
        #region diaglogOPTS
        var dialogOPTS = Dialogs.CreateInputDialog("Отклонить");
        var opts = dialogOPTS.AddString("Введите причину отклонения", true);
        dialogOPTS.Buttons.AddOkCancel();
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
                Dictionary<string, byte[]> signdata = new Dictionary<string, byte[]>();
                string forSign = "forsign: {\"address\": \"{serverAddress}\", \"login\": \"{login}\", \"password\": \"{password}\", \"document_id\": {document_id}, \"issigned\": {issigned}, \"pkcs7\": \"{pkcs7}\"}";
                
                string stringForSign = MultibankModule.PublicFunctions.Module.Remote.GNKString(databook, "reject", opts.Value);
                string stringForSign64 = Convert.ToBase64String(Encoding.Default.GetBytes(stringForSign));
                forSign = forSign.Replace("{serverAddress}", address).Replace("{login}", login).Replace("{password}", password.Value).Replace("{document_id}", _obj.Id.ToString()).Replace("{issigned}", "false").Replace("{pkcs7}", stringForSign64);
                Logger.Debug("String for sign: " + forSign);
                signdata.Add("1", Encoding.Default.GetBytes(forSign));
                var externalSign = ExternalSignatures.Sign(certificate.Value, signdata);
                var signedString = Convert.ToBase64String(externalSign.FirstOrDefault().Value);
                string status = MultibankModule.PublicFunctions.Module.Remote.RejectDocumentInMultibank(databook, opts.Value);
                var document = databook.Document;
                if (!status.Contains("\"success\":true"))
                {
                  document.VerificationState = document.VerificationStateAllowedItems.Where(x => x.Value.Contains("Error")).FirstOrDefault();
                  Dialogs.ShowMessage("Ошибка", status, MessageType.Error);
                }
                else document.VerificationState = document.VerificationStateAllowedItems.Where(x => x.Value.Contains("Completed")).FirstOrDefault();
              }
            }
          }
        }
      }
    }

    public virtual bool CanRejectmicros(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return true;
    }

    public virtual void Acceptmicros(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      var databook = micros.MultibankModule.IntegrationDatabooks.GetAll().Where(x => x.Document.Id == e.Entity.Id).FirstOrDefault();
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
              Dictionary<string, byte[]> signdata = new Dictionary<string, byte[]>();
              string forSign = "forsign: {\"address\": \"{serverAddress}\", \"login\": \"{login}\", \"password\": \"{password}\", \"issigned\": {issigned}, \"pkcs7\": \"{pkcs7}\"}";
              
              string stringForSign = Convert.ToBase64String(databook.Sign);
              forSign = forSign.Replace("{serverAddress}", address).Replace("{login}", login).Replace("{password}", password.Value).Replace("{issigned}", "true").Replace("{pkcs7}", stringForSign);
              Logger.Debug("String for sign: " + forSign);
              signdata.Add("1", Encoding.Default.GetBytes(forSign));
              var externalSign = ExternalSignatures.Sign(certificate.Value, signdata);
              var signedString = Convert.ToBase64String(externalSign.FirstOrDefault().Value);
              string status = MultibankModule.PublicFunctions.Module.Remote.ReturnSigningDocumentInMultibank(databook);
              var document = databook.Document;
              if (!status.Contains("\"success\":true"))
              {
                document.VerificationState = document.VerificationStateAllowedItems.Where(x => x.Value.Contains("Error")).FirstOrDefault();
                Dialogs.ShowMessage("Ошибка", status, MessageType.Error);
              }
              else document.VerificationState = document.VerificationStateAllowedItems.Where(x => x.Value.Contains("Completed")).FirstOrDefault();
            }
          }
        }
      }
    }

    public virtual bool CanAcceptmicros(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return true;
    }

    public virtual void Cancelmicros(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      var databook = micros.MultibankModule.IntegrationDatabooks.GetAll().Where(x => x.Document.Id == e.Entity.Id).FirstOrDefault();
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
              Dictionary<string, byte[]> signdata = new Dictionary<string, byte[]>();
              string forSign = "forsign: {\"address\": \"{serverAddress}\", \"login\": \"{login}\", \"password\": \"{password}\", \"issigned\": {issigned}, \"pkcs7\": \"{pkcs7}\"}";
              
              string stringForSign = MultibankModule.PublicFunctions.Module.Remote.GNKString(databook, "cancel", string.Empty);
              string stringForSign64 = Convert.ToBase64String(Encoding.Default.GetBytes(stringForSign));

              forSign = forSign.Replace("{serverAddress}", address).Replace("{login}", login).Replace("{password}", password.Value).Replace("{issigned}", "false").Replace("{pkcs7}", stringForSign64);
              Logger.Debug("String for sign: " + forSign);
              signdata.Add("1", Encoding.Default.GetBytes(forSign));
              var externalSign = ExternalSignatures.Sign(certificate.Value, signdata);
              var signedString = Convert.ToBase64String(externalSign.FirstOrDefault().Value);
              string status = MultibankModule.PublicFunctions.Module.Remote.CancelDocumentInMultibank(databook);
              var document = databook.Document;
              if (!status.Contains("\"success\":true"))
              {
                document.VerificationState = document.VerificationStateAllowedItems.Where(x => x.Value.Contains("Error")).FirstOrDefault();
                Dialogs.ShowMessage("Ошибка", status, MessageType.Error);
              }
              else document.VerificationState = document.VerificationStateAllowedItems.Where(x => x.Value.Contains("Completed")).FirstOrDefault();
            }
          }
        }
      }
    }

    public virtual bool CanCancelmicros(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return true;
    }

  }

}