using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Sungero.Core;
using Sungero.CoreEntities;
using RestSharp;
using RestSharp.Authenticators;
using micros.MultibankModule.EContract;

namespace micros.MultibankModule.Client
{
  partial class EContractActions
  {
    public virtual void OpenInMultibank(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      string url = micros.MultibankModule.PublicFunctions.Module.Remote.GetBuisnesUnitBoxForCompany(_obj.BusinessUnit).ExchangeService.InvoiceURLmicros;
      string document_id = micros.MultibankModule.IntegrationDatabooks.GetAll().Where(x => x.Document == _obj).FirstOrDefault().Guid;
      url = url.Replace("{document_id}", document_id);
      Hyperlinks.Open(url);
    }

    public virtual bool CanOpenInMultibank(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return true;
    }

    public virtual void UpdateFromMultibank(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      var databook = MultibankModule.IntegrationDatabooks.GetAll().Where(x => x.Document == _obj).FirstOrDefault();
      string jsonString = micros.MultibankModule.PublicFunctions.Module.Remote.GetMultibankJsonFromID(_obj).ToLower();
      //Logger.Debug("Contract Update" + jsonString);
      micros.MultibankModule.PublicFunctions.Module.Remote.UpdateContractFromJson(jsonString, databook);
      _obj.Save();
    }

    public virtual bool CanUpdateFromMultibank(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return true;
    }

    public virtual void SendToMultibank(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      _obj.Save();
      string error = MultibankModule.PublicFunctions.Module.Remote.EContractErrors(_obj);
      if (!String.IsNullOrEmpty(error)) Dialogs.ShowMessage(error, MessageType.Error);
      else
      {
        var databook = IntegrationDatabooks.GetAll(x => x.Document == _obj).FirstOrDefault();
        if (databook == null)
        {
          var data = micros.MultibankModule.PublicFunctions.Module.Remote.FillElectronicContract(_obj);
          var response = MultibankModule.PublicFunctions.Module.Remote.SendDocument("contract_doc", data, _obj);
          Logger.Debug("Send document response: " + response);
          if (response.Contains("\"success\":true")) databook = MultibankModule.PublicFunctions.Module.Remote.CreateDatabook(_obj, data, response);
          else Dialogs.ShowMessage("", MultibankModule.PublicFunctions.Module.Remote.GetMessageFromResponse(response), MessageType.Error);
        }
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
          IRestResponse response2 = client.Execute(request);
          if(response2.StatusCode != HttpStatusCode.OK) Dialogs.ShowMessage("Ошибка, пожалуйста проверьте корректность пароля", MessageType.Error);
          else
          {
            if (dialogCert.Show() == DialogButtons.Ok)
            {
              string pass = password.Value;
              if (pass.Contains(@"\")) pass = pass.Replace(@"\", @"\\");
              if (login.Contains(@"\")) login = login.Replace(@"\", @"\\");
              Dictionary<string, byte[]> signdata = new Dictionary<string, byte[]>();
              string forSign = "forsign: {\"address\": \"{serverAddress}\", \"login\": \"{login}\", \"password\": \"{password}\", \"document_id\": {document_id}, \"issigned\": {issigned}, \"pkcs7\": {pkcs7}}";
              
              string stringForSign = micros.MultibankModule.PublicFunctions.Module.Remote.GNKString(databook, "signing", String.Empty);
              forSign = forSign.Replace("{serverAddress}", address).Replace("{login}", login).Replace("{password}", pass).Replace("{document_id}", _obj.Id.ToString()).Replace("{issigned}", "false").Replace("{pkcs7}", stringForSign);
              Logger.Debug("String for sign: " + forSign.Replace(password.Value, "******"));
              signdata.Add("1", Encoding.UTF8.GetBytes(forSign));
              var externalSign = ExternalSignatures.Sign(certificate.Value, signdata);
              //var signedString = Convert.ToBase64String(externalSign.FirstOrDefault().Value);
              string status = MultibankModule.PublicFunctions.Module.Remote.SignInMultibank(databook);
              var document = databook.Document;
              if (!status.Contains("\"success\":true"))
              {
                document.VerificationState = document.VerificationStateAllowedItems.Where(x => x.Value.Contains("Error")).FirstOrDefault();
                Dialogs.ShowMessage("", MultibankModule.PublicFunctions.Module.Remote.GetMessageFromResponse(status), MessageType.Error);
              }
              else
              {
                var response = PublicFunctions.Module.Remote.SendByMultibank(databook);
                if (!response.Contains("\"success\":true")) Dialogs.ShowMessage("", MultibankModule.PublicFunctions.Module.Remote.GetMessageFromResponse(response), MessageType.Error);
                else
                {
                  document.VerificationState = document.VerificationStateAllowedItems.Where(x => x.Value.Contains("Completed")).FirstOrDefault();
                  string jsonString = micros.MultibankModule.PublicFunctions.Module.Remote.GetMultibankJsonFromID(_obj).ToLower();
                  micros.MultibankModule.PublicFunctions.Module.Remote.UpdateContractFromJson(jsonString.ToString(), databook);
                  _obj.Save();
                }
              }
            }
          }
        }
      }
    }

    public virtual bool CanSendToMultibank(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return true;
    }

    public virtual void DownloadJSon(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      _obj.Save();
      string error = MultibankModule.PublicFunctions.Module.Remote.EContractErrors(_obj);
      if (!String.IsNullOrEmpty(error)) Dialogs.ShowMessage(error, MessageType.Error);
      else
      {
        var data = micros.MultibankModule.PublicFunctions.Module.Remote.FillElectronicContract(_obj);
        var zip = micros.MultibankModule.PublicFunctions.Module.Remote.ExportData(data);
        zip.Export();
      }
    }

    public virtual bool CanDownloadJSon(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return true;
    }

  }

}