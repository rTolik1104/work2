using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using micros.MultibankSolution.IncomingTaxInvoice;
using micros;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace micros.MultibankSolution.Client
{
  partial class IncomingTaxInvoiceCollectionActions
  {
    public override void Sign(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      base.Sign(e);
      //var document = Sungero.Content.ElectronicDocuments.Get(e.Entity.Id);
      //micros.Multibank.PublicFunctions.Module.Remote.MultibankSign(document);
    }

    public override bool CanSign(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return base.CanSign(e);
    }

  }

  partial class IncomingTaxInvoiceActions
  {
    public virtual void SignInMultibankmicros(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      if (micros.Multibank.PublicFunctions.Module.Remote.IsSignedInMultibank(_obj))
        Dialogs.ShowMessage("Документ уже подписан");
      else
      {
        string url = micros.Multibank.Resources.OpenInvoicePage;
        string json = micros.Multibank.PublicFunctions.Module.GetJsonFromBody(_obj);
        string document_id = micros.Multibank.PublicFunctions.Module.Remote.GetJsonMultibankGuid(json);
        //string password = micros.Multibank.PublicFunctions.Module.Remote.GetJsonMultibankPassword(json);
        url = url.Replace("{document_id}", document_id);
        //url = url.Replace("{document_password_owner}", password);
        Hyperlinks.Open(url);
      }
      
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
      
      string json = Multibank.PublicFunctions.Module.FillInvoiceV2(_obj.LastVersion);
      string respond = json != "null" ? Multibank.PublicFunctions.Module.SendDocument(json) : "null";
      
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