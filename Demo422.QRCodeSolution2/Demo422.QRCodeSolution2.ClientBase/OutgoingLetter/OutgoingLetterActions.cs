using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Demo422.QRCodeSolution2.OutgoingLetter;
using System.IO;

namespace Demo422.QRCodeSolution2.Client
{
  partial class OutgoingLetterActions
  {
    public override void ConvertToPdf(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      base.ConvertToPdf(e);
      var documentID=_obj.Id;
      var path=@"E:\C#\repos\DirectumRX_beta\DirectumRX_beta\wwwroot\files\";
      var bodyId=Demo422.QRCodeSolution2.Functions.OutgoingLetter.Remote.GetPublicBodyID(_obj);
      path+=bodyId+".pdf";
      StreamReader reader=new StreamReader(_obj.LastVersion.PublicBody.Read());
      var bytes = default(byte[]);
      using (var memstream = new MemoryStream())
      {
          reader.BaseStream.CopyTo(memstream);
          bytes = memstream.ToArray();
      }
      File.WriteAllBytes(path,bytes);

    }

    public override bool CanConvertToPdf(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return base.CanConvertToPdf(e);
    }

  }

}