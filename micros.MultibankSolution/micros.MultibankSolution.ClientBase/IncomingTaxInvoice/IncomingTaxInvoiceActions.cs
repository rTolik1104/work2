using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using micros.MultibankSolution.IncomingTaxInvoice;
using micros;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace micros.MultibankSolution.Client
{
  partial class IncomingTaxInvoiceCollectionActions
  {
    public override void Sign(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      var invoice = MultibankSolution.IncomingTaxInvoices.Get(e.Entity.Id);
      if(invoice.Guidmicros != null && invoice.HasVersions)
      {
        //MultibankModule.PublicFunctions.Module.Remote.GetMultibankSignData(invoice);
        Signatures.Approve(invoice.LastVersion, "e-imzo");
        //var signatory = Signatures.Get(invoice.LastVersion).Last();
        //Logger.Debug("SignDataRX: " + Convert.ToBase64String(signatory.GetDataSignature()));
        MultibankModule.PublicFunctions.Module.Remote.ReturnSigningDocumentInMultibank(invoice);
      }
      else
        base.Sign(e);
    }

    public override bool CanSign(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return base.CanSign(e);
    }

  }

  partial class IncomingTaxInvoiceActions
  {
    public override void ShowSignatures(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      base.ShowSignatures(e);
    }

    public override bool CanShowSignatures(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return base.CanShowSignatures(e);
    }



    
    public virtual void OpenMultibankPublicmicros(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      string url = micros.MultibankModule.PublicFunctions.Module.Remote.GetIntegrationDataBookForCurrentUser().ExchangeService.InvoiceURLmicros;
      string document_id = _obj.Guidmicros;
      url = url.Replace("{document_id}", document_id);
      Hyperlinks.Open(url);
    }

    public virtual bool CanOpenMultibankPublicmicros(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return true;
    }

    public virtual void UpdateFromMultibankmicros(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      string jsonString = micros.MultibankModule.PublicFunctions.Module.Remote.GetMultibankJsonFromID(_obj);
      string oldJson = Encoding.Default.GetString(_obj.JSonDocumentmicros);
      //JObject test = JObject.Parse(jsonString);
      if (oldJson != jsonString)
        micros.MultibankModule.PublicFunctions.Module.Remote.UpdateInvoiceFromJson(jsonString.ToString(), _obj);
      _obj.UpdateStatusDatemicros = Calendar.Now;
      _obj.Save();
    }

    public virtual bool CanUpdateFromMultibankmicros(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return true;
    }

    public virtual void SignInMultibankmicros(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      //if (!micros.Multibank.PublicFunctions.Module.Remote.IsSignedInMultibank(_obj))
      //Dialogs.ShowMessage("Документ уже подписан");
      //else
      //{
      string url = micros.MultibankModule.PublicFunctions.Module.Remote.GetIntegrationDataBookForCurrentUser().ExchangeService.InvoiceURLmicros;
      //string json = micros.Multibank.PublicFunctions.Module.Remote.GetJsonFromBody(_obj);
      string document_id = _obj.Guidmicros;
      url = url.Replace("{document_id}", document_id);
      Hyperlinks.Open(url);
      //Hyperlinks
      //}
    }

    public virtual bool CanSignInMultibankmicros(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return true;
    }

    public virtual void SendToMultibankmicros(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      if(_obj.HasVersions != true)
      {
        e.AddError("Версия документа отсутствует.");
        return;
      }
      
      var authbook = micros.MultibankModule.AuthDatabooks.GetAll().Where(a => a.BusinessUnit == Sungero.Company.Employees.Current.Department.BusinessUnit).FirstOrDefault();
      //var authbook = micros.Multibank.AuthDatabooks.Get(2);
      
      if(DateTimeOffset.Now.ToUnixTimeSeconds() > Convert.ToInt32(authbook.ExpireAccess))
        MultibankModule.PublicFunctions.AuthDatabook.UpdateTokens(authbook, MultibankModule.PublicFunctions.Module.RefreshToken(Encoding.Default.GetString(authbook.RefreshToken)));

      string json = MultibankModule.PublicFunctions.Module.FillInvoiceV2(_obj.LastVersion);
      string respond = json != "null" ? MultibankModule.PublicFunctions.Module.SendDocument(json, Encoding.Default.GetString(authbook.AccessToken)) : "null";
      
      if(respond == "!Тут нужно поставить условия!")
        Dialogs.NotifyMessage("Черновик успешно создан.");
      else if(respond == "null")
        e.AddError("Ошибка: Возможно что расширение версий документа не соответствует формату Json");
      else
        e.AddWarning(respond);
    }

    public virtual bool CanSendToMultibankmicros(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return true;
    }
  }
}