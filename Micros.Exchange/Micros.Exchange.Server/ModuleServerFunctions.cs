using System;
using System.Collections.Generic;
using System.Linq;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Cms;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Cms;
using Sungero.Core;
using Sungero.CoreEntities;

namespace Micros.Exchange.Server
{
  public class ModuleFunctions
  {
    /// <summary>
    /// Сконвертировать открепленную подпись в прикрепленную.
    /// </summary>
    public byte[] ConverSignatureToAttached(byte[] signature, byte[] data)
    {
      var asn1Object = Asn1Object.FromByteArray(signature);
      var contentInfo = ContentInfo.GetInstance(asn1Object);
      var signedData = SignedData.GetInstance(contentInfo.Content);
      var newEncapContentInfo = new ContentInfo(CmsObjectIdentifiers.Data, new BerOctetString(data));
      var newSignedData = new SignedData(signedData.DigestAlgorithms, newEncapContentInfo, signedData.Certificates, signedData.CRLs, signedData.SignerInfos);
      var newContentInfo = new ContentInfo(contentInfo.ContentType, newSignedData);
      return newContentInfo.GetDerEncoded();
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="signature1"></param>
    /// <param name="signature2"></param>
    /// <returns></returns>
    public byte[] MergeSignatures(byte[] signature1, byte[] signature2)
    {
      var digestAlgorithms = new List<Asn1Encodable>();
      var certificates = new List<Asn1Encodable>();
      var crls = new List<Asn1Encodable>();
      var signers = new List<Asn1Encodable>();

      var signedData1 = SignedData.GetInstance(ContentInfo.GetInstance(Asn1Object.FromByteArray(signature1)).Content);

      if (signedData1.DigestAlgorithms != null)
        digestAlgorithms.AddRange(signedData1.DigestAlgorithms.ToArray());
      if (signedData1.Certificates != null)
        certificates.AddRange(signedData1.Certificates.ToArray());
      if (signedData1.CRLs != null)
        crls.AddRange(signedData1.CRLs.ToArray());
      if (signedData1.SignerInfos != null)
        signers.AddRange(signedData1.SignerInfos.ToArray());

      var signedData2 = SignedData.GetInstance(ContentInfo.GetInstance(Asn1Object.FromByteArray(signature2)).Content);

      if (signedData2.DigestAlgorithms != null)
        digestAlgorithms.AddRange(signedData2.DigestAlgorithms.ToArray());
      if (signedData2.Certificates != null)
        certificates.AddRange(signedData2.Certificates.ToArray());
      if (signedData2.CRLs != null)
        crls.AddRange(signedData2.CRLs.ToArray());
      if (signedData2.SignerInfos != null)
        signers.AddRange(signedData2.SignerInfos.ToArray());


      var newSignedData = new SignedData(digestAlgorithms.Any() ? new DerSet(digestAlgorithms.Distinct().ToArray()) : null,
                                         signedData1.EncapContentInfo,
                                         certificates.Any() ? new DerSet(certificates.Distinct().ToArray()) : null,
                                         crls.Any() ? new DerSet(crls.Distinct().ToArray()) : null,
                                         signers.Any() ? new DerSet(signers.Distinct().ToArray()) : null);

      return new ContentInfo(CmsObjectIdentifiers.SignedData, newSignedData).GetDerEncoded();
    }
  }
}