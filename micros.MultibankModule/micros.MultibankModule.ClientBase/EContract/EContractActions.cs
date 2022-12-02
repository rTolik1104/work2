using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using micros.MultibankModule.EContract;

namespace micros.MultibankModule.Client
{
  partial class EContractActions
  {
    public virtual void SendToMultibank(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      _obj.Save();
      string error = MultibankModule.PublicFunctions.Module.Remote.EContractErrors(_obj);
      if (!String.IsNullOrEmpty(error)) Dialogs.ShowMessage(error, MessageType.Error);
      else
      {
        var data = micros.MultibankModule.PublicFunctions.Module.Remote.FillElectronicContract(_obj);
        var response = MultibankModule.PublicFunctions.Module.Remote.SendDocument("contract_doc", data, _obj);
        if (response.Contains("\"success\":true"))
        {
          
        }
        else
        {
          Dialogs.ShowMessage("", MultibankModule.PublicFunctions.Module.Remote.GetMessageFromResponse(response), MessageType.Error);
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