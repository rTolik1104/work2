using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Micros.Exchange.ExchangeDocument;

namespace Micros.Exchange.Server
{
  partial class ExchangeDocumentFunctions
  {

    /// <summary>
    /// 
    /// </summary>
    [Remote(IsPure = true)]
    public IZip ExportAttachedSignature()
    {
      var sugnatures = Signatures.Get(_obj.LastVersion);

      if(sugnatures.Count() == 0)
        return null;
      
      byte[] finalSignature = null;

      foreach(var sugnature in sugnatures.OrderBy(s => s.SigningDate))
      {
        if(finalSignature == null)
        {
          finalSignature = sugnature.GetDataSignature();
          continue;
        }
        
        finalSignature = Exchange.Functions.Module.MergeSignatures(finalSignature, sugnature.GetDataSignature());
      }

      using (var ms = new System.IO.MemoryStream())
      {
        _obj.LastVersion.Body.Read().CopyTo(ms);
        finalSignature = Exchange.Functions.Module.ConverSignatureToAttached(finalSignature, ms.ToArray());
      }

      var zip = Sungero.Core.Zip.Create();
      zip.Add(finalSignature, "merged", "sign");
      zip.Save("signatures.zip");
      return zip;
    }

  }
}