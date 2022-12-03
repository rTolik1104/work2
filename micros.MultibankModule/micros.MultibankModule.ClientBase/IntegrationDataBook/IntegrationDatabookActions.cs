using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using micros.MultibankModule.IntegrationDatabook;

namespace micros.MultibankModule.Client
{
  partial class IntegrationDatabookActions
  {
    public virtual void DownloadAction(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      var zip = MultibankModule.PublicFunctions.Module.Remote.ExportAction(_obj);
      zip.Export();
    }

    public virtual bool CanDownloadAction(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return true;
    }

    public virtual void DownloadSign(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      var zip=MultibankModule.PublicFunctions.Module.Remote.ExportSign(_obj);
      zip.Export();
    }

    public virtual bool CanDownloadSign(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return true;
    }

    public virtual void DownloadJson(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      var zip=MultibankModule.PublicFunctions.Module.Remote.ExportJson(_obj);
      zip.Export();
    }

    public virtual bool CanDownloadJson(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return true;
    }

  }

}