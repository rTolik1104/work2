using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using System.Text;

namespace micros.EImzoSolution.Module.Docflow.Server
{
  partial class ModuleFunctions
  {
    /// <summary>
    /// Получить отметку об ЭЦП для сертификата из подписи.
    /// </summary>
    /// <param name="signature">Подпись.</param>
    /// <returns>Изображение отметки об ЭЦП для сертификата в виде HTML.</returns>
    /// <description>
    /// В отметку добавлены дата и время подписания.
    /// </description>
    public override string GetSignatureMarkForCertificateAsHtml(Sungero.Domain.Shared.ISignature signature)
    {
      #region этот код не используется
      /*if (signature == null)
        return string.Empty;

      var certificate = signature.SignCertificate;
      if (certificate == null)
        return string.Empty;

      var certificateSubject = this.GetCertificateSubject(signature);
      //var signatoryTitle = signature.SignatoryFullName;

      var signatoryFullName = string.Format("{0} {1}", certificateSubject.Surname, certificateSubject.GivenName).Trim();
      //if (string.IsNullOrEmpty(signatoryFullName))
      signatoryFullName = certificateSubject.CounterpartyName;
      var signatoryTitle = signature.Signatory.Name;

      string html = Resources.HtmlStampTemplateForCertificateCustom;
      html = html.Replace("{SignatoryFullName}", signatoryFullName);
      html = html.Replace("{SignatoryTitle}", signatoryTitle.ToString());
      html = html.Replace("{Thumbprint}", certificate.Thumbprint.ToLower());
      var validity = string.Format("{0} {1} {2} {3}",
                                   Company.Resources.From,
                                   certificate.NotBefore.Value.ToShortDateString(),
                                   Company.Resources.To,
                                   certificate.NotAfter.Value.ToShortDateString());

      html = html.Replace("{Validity}", validity);
      html = html.Replace("{SigningDate}", signature.SigningDate.ToString("g"));
      
      return html;*/
      #endregion
      
      int gmt = 5;
      
      if (signature == null)
        return string.Empty;

      var signatoryFullName = signature.SignatoryFullName;
      var signatoryId = signature.Signatory.Id;
      var current = Sungero.Company.Employees.Get(signatoryId).JobTitle;
      var signatoryTitle = Sungero.Company.Employees.Get(signatoryId).JobTitle;
      //var signatoryTitle = string.Format("{0}", Sungero.Company.Employees.Current.JobTitle);
      var validity = string.Format("{0} {1} {2} {3}", Sungero.Company.Resources.From, signature.SignCertificate.NotBefore.Value.ToShortDateString(), Sungero.Company.Resources.To, signature.SignCertificate.NotAfter.Value.ToShortDateString());
      
      string html = EimzoModule.Resources.HtmlStampTemplateForCertificateCustom;
      html = html.Replace("{SignatoryFullName}", signatoryFullName);
      //html = html.Replace("{SignatoryId}", signatoryId.ToString());
      html = html.Replace("{SigningDate}", signature.SigningDate.AddHours(gmt).ToString("G"));
      html = html.Replace("{Thumbprint}",signature.SignCertificate.Thumbprint.ToString());
      html = html.Replace("{SignatoryTitle}",signatoryTitle.ToString());
      html = html.Replace("{Validity}", validity);
      
      return html;
    }
    
    
    /// <summary>
    /// Получить отметку об ЭП для подписи.
    /// </summary>
    /// <param name="signature">Подпись.</param>
    /// <returns>Изображение отметки об ЭП для подписи в виде HTML.</returns>
    /// <description>
    /// В отметку добавлены дата и время подписания.
    /// </description>
    public override string GetSignatureMarkForSimpleSignatureAsHtml(Sungero.Domain.Shared.ISignature signature)
    {
      if (signature == null)
        return string.Empty;
      
      int gmt = 5;
      
      //For QR code
      var docId=signature.Entity.Id;
      //var docTypeId=Sungero.Docflow.OfficialDocuments.GetAll(x=>x.Id==docId).First().DocumentKind.DocumentType.DocumentTypeGuid;
      
      var signatoryFullName = signature.SignatoryFullName;
      var signatoryId = signature.Signatory.Id;
      var current = Sungero.Company.Employees.Get(signatoryId).JobTitle;
      var signatoryTitle = Sungero.Company.Employees.Get(signatoryId).JobTitle;
      //var signatoryTitle = string.Format("{0}", Sungero.Company.Employees.Current.JobTitle);
      
      
      string html = EimzoModule.Resources.HtmlStampTemplateForSignatureCustom;
      var signatoryJobTitle=Sungero.Company.Employees.Get(signature.Signatory.Id).JobTitle;
      var signatoryType=signature.SignatureType.ToString();
      html=html.Replace("{SignatoryJobTitle}",signatoryJobTitle.ToString());
      html=html.Replace("{SignatoryFullName}",signatoryFullName);
      //html=html.Replace("{SignatoryType}",signatoryType);
      html=html.Replace("{SignatoryDate}",signature.SigningDate.AddHours(gmt).ToString("G"));
      html=html.Replace("{ImageQR}",Demo422.QRCodeSol.PublicFunctions.Module.GetDocumentQRCode(docId));
      
      #region Tolkin
      //      var signatoryFullName=signature.SignatoryFullName;
      //      var user=Sungero.Company.Employees.GetAll(x=>x.Id==signature.Signatory.Id).First();
      //      var signatoryJobTitle=user.JobTitle;
      //      var signatoryType=signature.SignatureType.ToString();
//
      //      string html=System.IO.File.ReadAllText(@"C:\stamp.txt", Encoding.UTF8);
      //      html=html.Replace("{SignatoryJobTitle}",signatoryJobTitle.ToString());
      //      html=html.Replace("{SignatoryFullName}",signatoryFullName);
      //      html=html.Replace("{SignatoryType}",signatoryType);
      //      html=html.Replace("{SignatoryDate}",signature.SigningDate.AddHours(gmt).ToString("G"));
      #endregion

      //Logger.Debug(html);
      return html;
    }
  }
}