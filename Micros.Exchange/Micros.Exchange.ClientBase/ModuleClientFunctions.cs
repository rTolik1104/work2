using System;
using System.Collections.Generic;
using System.Linq;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Cms;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Cms;
using Sungero.Core;
using Sungero.CoreEntities;

namespace Micros.Exchange.Client
{
  public class ModuleFunctions
  {
    /// <summary>
    /// Получить тело документа из прикрепленной подписи.
    /// </summary>
    public byte[] GetAttachedBodyFromSignature(byte[] signature)
    {
      var asn1Object = Asn1Object.FromByteArray(signature);
      var contentInfo = ContentInfo.GetInstance(asn1Object);
      var signedData = SignedData.GetInstance(contentInfo.Content);
      return ((Asn1OctetString)signedData.EncapContentInfo.Content).GetOctets();
    }
    
    /// <summary>
    /// Сконвертировать прикрепленную подпись в открепленную.
    /// </summary>
    public byte[] ConverSignatureToDeattached(byte[] signature)
    {
      var asn1Object = Asn1Object.FromByteArray(signature);
      var contentInfo = ContentInfo.GetInstance(asn1Object);
      var signedData = SignedData.GetInstance(contentInfo.Content);
      var newEncapContentInfo = new ContentInfo(CmsObjectIdentifiers.Data, null);
      var newSignedData = new SignedData(signedData.DigestAlgorithms, newEncapContentInfo, signedData.Certificates, signedData.CRLs, signedData.SignerInfos);
      var newContentInfo = new ContentInfo(contentInfo.ContentType, newSignedData);
      return newContentInfo.GetDerEncoded();
    }
    
    public System.Collections.Generic.IEnumerable<byte[]> SplitSignatures(byte[] signature)
    {
      var result = new List<byte[]>();

      var signedData = SignedData.GetInstance(ContentInfo.GetInstance(Asn1Object.FromByteArray(signature)).Content);
      // Эти данные могут быть задублированы если подписей несколько, RX-у от этого нехорошо, поэтому удаляем дубли.
      var digestAlgorithms = signedData.DigestAlgorithms != null ? new DerSet(signedData.DigestAlgorithms.OfType<Asn1Encodable>().Distinct().ToArray()) : null;
      var certificates = signedData.Certificates != null ? new DerSet(signedData.Certificates.OfType<Asn1Encodable>().Distinct().ToArray()) : null;
      var crls = signedData.CRLs != null ? new DerSet(signedData.CRLs.OfType<Asn1Encodable>().Distinct().ToArray()) : null;

      foreach(var signerInfo in signedData.SignerInfos.ToArray())
      {
        var newSignedData = new SignedData(digestAlgorithms, signedData.EncapContentInfo, certificates, crls, new DerSet(signerInfo));
        result.Add(new ContentInfo(CmsObjectIdentifiers.SignedData, newSignedData).GetDerEncoded());
      }

      return result;
    }
  }
}