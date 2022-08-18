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
    ///Генерация QR кода
    ///<returns>Html код с изображением.</returns>
    [Public]
    public string GetDocumentQRCode(int docId){
      var result = "<img src='data:image/png;base64,";
      var password="";
      string url=$"https://localhost:7015/Public/id={docId}&&pass={password}";
      
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
      
      result+="' style='width: 120px;padding-left: 50px;'/>";
      return result;
    }
    
    //Преоброзования в base64
    public string ImageToBase64(byte[] imageBytes){
      return Convert.ToBase64String(imageBytes);
    }
    
  }
}