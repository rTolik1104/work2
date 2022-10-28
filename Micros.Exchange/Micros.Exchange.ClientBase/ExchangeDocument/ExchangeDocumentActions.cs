using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sungero.Core;
using Sungero.CoreEntities;
using Micros.Exchange.ExchangeDocument;

namespace Micros.Exchange.Client
{
  partial class ExchangeDocumentActions
  {
    public virtual void ExportAttachedSignature(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      var zip = Exchange.Functions.ExchangeDocument.Remote.ExportAttachedSignature(_obj);
      zip.Export();
    }

    public virtual bool CanExportAttachedSignature(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return true;
    }

    public virtual void ImportAttachedSignature(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      var dialog = Dialogs.CreateInputDialog("Выберите файл подписи");
      dialog.Buttons.AddOkCancel();
      var file = dialog.AddFileSelect("Файл подписи", true);
      if (dialog.Show() == DialogButtons.Ok) {
        var sign64 = Encoding.UTF8.GetString(file.Value.Content);
        var sign = Convert.FromBase64String(sign64);
        var body = Exchange.Functions.Module.GetAttachedBodyFromSignature(sign);
        using(var ms = new MemoryStream(body)) {
          _obj.CreateVersionFrom(ms, "txt");
          _obj.Save();
        }
        sign = Exchange.Functions.Module.ConverSignatureToDeattached(sign);
        int i = 1;
        foreach(var s in Exchange.Functions.Module.SplitSignatures(sign))
        {
          Signatures.Import(_obj, SignatureType.Endorsing, "multibank" + i, s, Calendar.UserNow, _obj.LastVersion);
          i++;
        }
      }
    }

    public virtual bool CanImportAttachedSignature(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return true;
    }

  }

}