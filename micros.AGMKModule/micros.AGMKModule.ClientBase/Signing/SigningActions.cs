using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using micros.AGMKModule.Signing;

namespace micros.AGMKModule.Client
{
  partial class SigningActions
  {
    public virtual void ForRevision(Sungero.Workflow.Client.ExecuteResultActionArgs e)
    {
      // Валидация заполненности активного текста.
      if (_obj.ActiveText == null)
      {
        e.AddError("Заполните текст доработки");
        e.Cancel();
      }
    }

    public virtual bool CanForRevision(Sungero.Workflow.Client.CanExecuteResultActionArgs e)
    {
      return true;
    }

    public virtual void Signing(Sungero.Workflow.Client.ExecuteResultActionArgs e)
    {
      
      var document = _obj.DocumentGroup.OfficialDocuments.First();
      if (!document.HasVersions)
      {
        e.AddError("Проверьте версию документа");
        e.Cancel();
      }
      Signatures.Approve(_obj.DocumentGroup.OfficialDocuments.First().LastVersion, "");
      //      var document = _obj.DocumentGroup.OfficialDocuments.First();
      //      var versionId = document.LastVersion.Id;
      //      var asyncConvertToPdf = Sungero.Docflow.AsyncHandlers.ConvertDocumentToPdf.Create();
      //      asyncConvertToPdf.DocumentId = document.Id;
      //      asyncConvertToPdf.VersionId = versionId;
      //      asyncConvertToPdf.UserId = Users.Current.Id;
      //      asyncConvertToPdf.ExecuteAsync();

      var signature = Sungero.Docflow.PublicFunctions.OfficialDocument.GetSignatureForMark(document, document.LastVersion.Id);
      string signatureString = Sungero.Docflow.PublicFunctions.Module.GetSignatureMarkForSimpleSignatureAsHtml(signature);
      micros.AGMKModule.PublicFunctions.Module.GeneratePublicBodyWithSignatureMark(document, document.LastVersion.Id, signatureString);
    }

    public virtual bool CanSigning(Sungero.Workflow.Client.CanExecuteResultActionArgs e)
    {
      return true;
    }

  }


}