using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Json;
using System.Net;
using System.IO;
using System.Text;

namespace micros.MicrosSetting.Server
{
  public class ModuleFunctions
  {
    /// <summary>
    /// Находит и возвращает все сертификаты в системе.
    /// </summary>
    /// <returns>Сертификаты пользователя</returns>
    [Remote]
    public IQueryable<ICertificate> GetAllCertificates()
    {
      return Certificates.GetAll();
    }
    
    [Remote]
    public static ICertificate CreateCertificate()
    {
      return Certificates.Create();
    }
    
    [Remote, Public]
    public bool AddProductAndService()
    {
      try
      {
        int dataReceiver = 0;
        string readedStream;
        WebRequest request = WebRequest.Create("https://api.multibank.uz/api/gnk/cl-api/our_all");
        request.Method = "Get";

        request.PreAuthenticate = true;
        HttpWebResponse response = (HttpWebResponse)request.GetResponse();
        using(Stream stream = response.GetResponseStream())
        {
          using(StreamReader reader = new StreamReader(stream))
          {
            readedStream = reader.ReadToEnd();
          }
        }
        response.Close();
        
        JObject json = JObject.Parse(readedStream);
        
        var dataList = json["data"].ToArray();
        foreach(var item in dataList)
        {
          var valueList = item.ToList();
          var codeClass = valueList[0].ToList()[0].ToString();
          var codeGroup = valueList[1].ToList()[0].ToString();
          var typeExist = Functions.TypeOfProductOrService.GetAllTypeOfProductsOrServices();
          var posExist = Functions.ProductAndService.GetAllProductsOrServices();
          
          if(posExist.Where(pos => pos.ClassCode == codeClass).Count() == 0)
          {
            var productOrService = Functions.ProductAndService.CreateProductOrService();
            productOrService.ClassCode = valueList[0].ToList()[0].ToString();
            productOrService.NameRu = valueList[2].ToList()[0].ToString();
            productOrService.NameUz = valueList[4].ToList()[0].ToString();
            productOrService.Name = productOrService.ClassCode + " - " + productOrService.NameRu;
            if(typeExist.Where(t => t.GroupCode == codeGroup).Count() == 0)
            {
              var typeOfProductOrService = Functions.TypeOfProductOrService.CreateTypeOfProductOrService();
              typeOfProductOrService.GroupCode = valueList[1].ToList()[0].ToString();
              typeOfProductOrService.NameRu = valueList[3].ToList()[0].ToString();
              typeOfProductOrService.NameUz = valueList[5].ToList()[0].ToString();
              typeOfProductOrService.Save();
              productOrService.GroupCode = typeOfProductOrService;
            }
            else
            {
              var type = typeExist.Where(t => t.GroupCode == codeGroup).FirstOrDefault();
              if(type.NameRu != valueList[3].ToList()[0].ToString() &&
                 type.NameUz != valueList[5].ToList()[0].ToString())
              {
                type.NameRu = valueList[3].ToList()[0].ToString();
                type.NameUz = valueList[5].ToList()[0].ToString();
                type.Save();
              }
              productOrService.GroupCode = type;
            }
            productOrService.Save();
          }
          else if(posExist.Where(pos => pos.ClassCode == codeClass).Count() > 0)
          {
            var productOrServiceExist = posExist.Where(pos => pos.ClassCode == codeClass).FirstOrDefault();
            if(productOrServiceExist.ClassCode != valueList[0].ToList()[0].ToString() ||
               productOrServiceExist.NameRu != valueList[2].ToList()[0].ToString() ||
               productOrServiceExist.NameUz != valueList[4].ToList()[0].ToString())
            {
              var productOrService = Functions.ProductAndService.CreateProductOrService();
              productOrService.ClassCode = valueList[0].ToList()[0].ToString();
              productOrService.NameRu = valueList[2].ToList()[0].ToString();
              productOrService.NameUz = valueList[4].ToList()[0].ToString();
              productOrService.Name = productOrService.ClassCode + " - " + productOrService.NameRu;
              if(typeExist.Where(t => t.GroupCode == codeGroup).Count() == 0)
              {
                var typeOfProductOrService = Functions.TypeOfProductOrService.CreateTypeOfProductOrService();
                typeOfProductOrService.GroupCode = valueList[1].ToList()[0].ToString();
                typeOfProductOrService.NameRu = valueList[3].ToList()[0].ToString();
                typeOfProductOrService.NameUz = valueList[5].ToList()[0].ToString();
                typeOfProductOrService.Save();
                productOrService.GroupCode = typeOfProductOrService;
              }
              else
              {
                var type = typeExist.Where(t => t.GroupCode == codeGroup).FirstOrDefault();
                if(type.NameRu != valueList[3].ToList()[0].ToString() ||
                   type.NameUz != valueList[5].ToList()[0].ToString())
                {
                  type.NameRu = valueList[3].ToList()[0].ToString();
                  type.NameUz = valueList[5].ToList()[0].ToString();
                  type.Save();
                  productOrService.GroupCode = type;
                }
              }
              
              productOrService.Save();
            }
          }
          dataReceiver++;
        }
        
        if(dataReceiver > 0)
        {
          return true;
        }
        else
        {
          return false;
        }
      }
      catch (Exception e)
      {
        var smt = e;
        return false;
      }
    }
  }
}