using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using RestSharp;
using RestSharp.Authenticators;
using System.Text;
using System.IO;
using Newtonsoft.Json;

namespace micros.Multibank.Client
{
  public class ModuleFunctions
  {

    /// <summary>
    /// 
    /// </summary>
    public virtual void Function1()
    {
      var document = Sungero.FinancialArchive.IncomingTaxInvoices.Create();
      document.Name = Calendar.Now.ToString();
      document.BusinessUnit = Sungero.Company.BusinessUnits.GetAll().FirstOrDefault();
      document.Save();
      //micros.Multibank.PublicFunctions.Module.Remote.;
      
      document.Show();
    }
    [Public]
    public string SendToEimzo(string incomingData)
    {
      //Отправка закпроса в E-imzo
      var client = new RestClient("/service/cryptapi HTTP/1.1");
      var request = new RestRequest(Method.GET);
      request.AddParameter("Host", "127.0.0.1");
      request.AddParameter("Connection", "Upgrade");
      request.AddParameter("Pragma", "no-cache");
      request.AddParameter("Cache-Control", "no-cache");
      request.AddParameter("Upgrade", "websocket");
      request.AddParameter("Origin", "http://127.0.0.1");
      request.AddParameter("Sec-WebSocket-Version", "13");
      request.AddParameter("Accept-Encoding", "gzip, deflate, br");
      request.AddParameter("Sec-WebSocket-Key", "cache");
      request.AddParameter("Content-Length", incomingData.Length);
      
      return null;
    }
  }
}