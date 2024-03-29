﻿using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using System.Text;
using System.Data;
using System.IO;

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
    /// Отметка изменена на QR код.
    /// </description>
    public override string GetSignatureMarkForCertificateAsHtml(Sungero.Domain.Shared.ISignature signature)
    {
      if (signature == null)
        return string.Empty;
      var isActive=Convert.ToBoolean(CheckActive());
      
      if(isActive)
      {
        //For QR code
        var docId=signature.Entity.Id;
        var signName=signature.SignCertificate.SubjectName;
        var thumbprint=signature.SignCertificate.Thumbprint;
        var signDate=signature.SigningDate;
        
        var document=Sungero.Docflow.OfficialDocuments.GetAll(x=>x.Id==docId).First();
        if(document.DocumentKind.DocumentType.DocumentFlow.Value.ToString()=="Outgoing")
        {
          var bodyHash="";
          using (var command = SQL.GetCurrentConnection().CreateCommand())
          {
            command.CommandText = string.Format(Queries.Module.SelectHashCode, docId);
            var obj=command.ExecuteScalar();
            bodyHash=obj.ToString();
          }
          
          var password=bodyHash.Substring(0,bodyHash.Length-1);
          using (var command = SQL.GetCurrentConnection().CreateCommand())
          {
            command.CommandText = string.Format(Queries.Module.SetPassword, password, docId);
            command.ExecuteNonQuery(); 
          }
          
          using (var command = SQL.GetCurrentConnection().CreateCommand())
          {
            command.CommandText = string.Format(Queries.Module.InsertEimzoData, docId, signName,thumbprint, signDate);
            command.ExecuteNonQuery(); 
          }
          
          string htmlT = EimzoModule.Resources.HtmlStampTemplateForCertificateCustom;
          htmlT=htmlT.Replace("{ImageQR}",Demo422.QRCodeSol.PublicFunctions.Module.GetDocumentQRCodePublic(docId));
          
          return htmlT;
        }
        string html = EimzoModule.Resources.HtmlStampTemplateForCertificateCustom;
        html=html.Replace("{ImageQR}",Demo422.QRCodeSol.PublicFunctions.Module.GetDocumentQRCode(docId, document.DocumentKind.DocumentType.DocumentTypeGuid));
        
        return html;
      }
      return base.GetSignatureMarkForCertificateAsHtml(signature);
    }
    
    
    /// <summary>
    /// Получить отметку об ЭП для подписи.
    /// </summary>
    /// <param name="signature">Подпись.</param>
    /// <returns>Изображение отметки об ЭП для подписи в виде HTML.</returns>
    /// <description>
    /// Отметка изменена на QR код.
    /// </description>
    public override string GetSignatureMarkForSimpleSignatureAsHtml(Sungero.Domain.Shared.ISignature signature)
    {
      if (signature == null)
        return string.Empty;
      var isActive=Convert.ToBoolean(CheckActive());
      if(isActive)
      {
        var docId=signature.Entity.Id;
        var docTypeId=Sungero.Docflow.OfficialDocuments.GetAll(x=>x.Id==docId).First().DocumentKind.DocumentType.DocumentTypeGuid;
        
        string html = EimzoModule.Resources.HtmlStampTemplateForSignatureCustom;
        html=html.Replace("{ImageQR}",Demo422.QRCodeSol.PublicFunctions.Module.GetDocumentQRCode(docId,docTypeId));
        
        return html;
      }
      return base.GetSignatureMarkForSimpleSignatureAsHtml(signature);
    }
    
    public override Sungero.Docflow.Structures.OfficialDocument.СonversionToPdfResult GeneratePublicBodyWithSignatureMark(Sungero.Docflow.IOfficialDocument document, int versionId, string signatureMark)
    {
      base.GeneratePublicBodyWithSignatureMark(document, versionId, signatureMark);
      var active=CheckActive();
      if(active=="True" && document.DocumentKind.DocumentType.DocumentFlow.Value.ToString()=="Outgoing")
      {
        try{
          var programmPath=GetPath();
          var path=$@"{programmPath}\";
          var bodyId=GetPublicBodyId(document);
          path+=bodyId+".pdf";
          StreamReader reader=new StreamReader(document.LastVersion.PublicBody.Read());
          var bytes=default(byte[]);
          using(var memstream=new MemoryStream())
          {
            reader.BaseStream.CopyTo(memstream);
            bytes=memstream.ToArray();
          }
          File.WriteAllBytes(path,bytes);
        }
        catch(Exception ex)
        {
          Logger.Error(ex.ToString());
        }
      }
      return Sungero.Docflow.Structures.OfficialDocument.СonversionToPdfResult.Create();
    }
    
    private string GetPublicBodyId(Sungero.Docflow.IOfficialDocument document)
    {
      string bodyId="";
      using (var command=SQL.GetCurrentConnection().CreateCommand())
      {
        command.CommandText=string.Format(Queries.Module.GetPublicBodyId,document.Id);
        var obj=command.ExecuteScalar();
        bodyId=obj.ToString();
      }
      return bodyId;
    }
    
    private string GetPath()
    {
      string path="";
      using (var command=SQL.GetCurrentConnection().CreateCommand())
      {
        command.CommandText=string.Format(Queries.Module.GetPath);
        var obj=command.ExecuteScalar();
        path=obj.ToString();
      }
      return path;
    }
    
    private string CheckActive()
    {
      string active="";
      using (var command=SQL.GetCurrentConnection().CreateCommand())
      {
        command.CommandText=string.Format(Queries.Module.CheckActive);
        var obj=command.ExecuteScalar();
        active=obj.ToString();
      }
      return active;
    }
  }
}