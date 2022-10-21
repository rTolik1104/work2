using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using System.Drawing;
using QRCoder;
using System.IO;
using System.Net;

namespace Demo422.QRCodeSol.Server
{
  public class ModuleFunctions
  {
    ///Генерация QR кода для публичной ссылки
    ///<returns>Html код с изображением.</returns>
    [Public]
    public string GetDocumentQRCodePublic(int docId){
      var result = "<img src='data:image/png;base64,";
      var publicHost="";
      var password="";
      
      using (var command = SQL.GetCurrentConnection().CreateCommand())
      {
        command.CommandText = string.Format(Queries.Module.GetPassword, docId);
        var obj=command.ExecuteScalar();
        password=obj.ToString();
      }
      using(var command=SQL.GetCurrentConnection().CreateCommand())
      {
        command.CommandText=string.Format(Queries.Module.GetPublicHost);
        var obj=command.ExecuteScalar();
        publicHost=obj.ToString();
      }
      
      string url=$"{publicHost}/Files/Public/id={docId}&&pass={password}";

      QRCodeGenerator qrGenerator = new QRCodeGenerator();
      var qrCodeData = qrGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q);
      QRCoder.QRCode qrCode = new QRCoder.QRCode(qrCodeData);
      Bitmap qrCodeImage = qrCode.GetGraphic(20);
      
      using (MemoryStream ms = new MemoryStream())
      {
          qrCodeImage.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
          byte[] byteImage = ms.ToArray();
          var image = this.ImageToBase64(byteImage);
          result += image;
      }
      result+="' style='width: 150px;'/>";
      return result;
    }
    
    //Преоброзования в base64
    public string ImageToBase64(byte[] imageBytes){
      return Convert.ToBase64String(imageBytes);
    }
    
    
    ///Генерация QR кода для внутренних документов
    ///<returns>Html код с изображением.</returns>
    [Public]
    public string GetDocumentQRCode(int docId, string docTypeId){
      var result = "<img src='data:image/png;base64,";
      string localHost="";
      
      using(var command=SQL.GetCurrentConnection().CreateCommand())
      {
        command.CommandText=string.Format(Queries.Module.GetLocalHostAddress);
        var obj=command.ExecuteScalar();
        localHost=obj.ToString();
      }
      string url=$"{localHost}/#/card/{docTypeId}/{docId}";
      
      QRCodeGenerator qrGenerator = new QRCodeGenerator();
      var qrCodeData = qrGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q);
      QRCoder.QRCode qrCode = new QRCoder.QRCode(qrCodeData);
      Bitmap qrCodeImage = qrCode.GetGraphic(20);
      
      using (MemoryStream ms = new MemoryStream())
      {
          qrCodeImage.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
          byte[] byteImage = ms.ToArray();
          var image = this.ImageToBase64(byteImage);
          result += image;
      }
      result+="' style='width: 150px;'/>";
      return result;
    }
  }
}