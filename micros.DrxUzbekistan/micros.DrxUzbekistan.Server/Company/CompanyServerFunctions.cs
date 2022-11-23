using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using micros.DrxUzbekistan.Company;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using RestSharp.Authenticators;

namespace micros.DrxUzbekistan.Server
{
  partial class CompanyFunctions
  {
    public string GetInnGNK()
    {
      string login=string.Empty;
      string password=string.Empty;
      
      using(var command=SQL.GetCurrentConnection().CreateCommand())
      {
        command.CommandText=Queries.Company.SelectMultibakData;
        var data=command.ExecuteReader();
        
        while(data.Read())
        {
          login=data.GetValue(1).ToString();
          password=data.GetValue(2).ToString();
        }
      }
        
      var client = new RestClient("https://api.multibank.uz/api/check_contragent/v1/external/gnk/" + _obj.TIN + "?refresh=1");
      //client.Authenticator = new HttpBasicAuthenticator("Micros24", "~AGJcw@Fxwvh");
      client.Authenticator = new HttpBasicAuthenticator(login, password);
      var request = new RestRequest(Method.GET);
      IRestResponse response = client.Execute(request);
      return (int)response.StatusCode == 200 ? response.Content : null;
    }
    
    public string GetInnGoscomstat()
    {
      string login=string.Empty;
      string password=string.Empty;
      
      using(var command=SQL.GetCurrentConnection().CreateCommand())
      {
        command.CommandText=Queries.Company.SelectMultibakData;
        var data=command.ExecuteReader();
        
        while(data.Read())
        {
          login=data.GetValue(1).ToString();
          password=data.GetValue(2).ToString();
        }
      }
      var client = new RestClient("https://api.multibank.uz/api/check_contragent/v1/external/stat/" + _obj.TIN + "?refresh=1");
      client.Authenticator = new HttpBasicAuthenticator(login, password);
      var request = new RestRequest(Method.GET);
      IRestResponse response = client.Execute(request);
      return (int)response.StatusCode == 200 ? response.Content : null;
    }
    
    [Public]
    public bool FillFromServicemicrosServer()
    {
      JObject jsonGnk=new JObject();
      try{
        jsonGnk = JObject.Parse(GetInnGNK());
      }
      catch{
        return false;
      }
      
      JObject jsonStat;
      string strStat = GetInnGoscomstat();
      
      try
      {
        if(jsonGnk != null)
        {
          _obj.Name = jsonGnk.SelectToken("data.gnk_company_name").ToString();
          _obj.LegalName = jsonGnk.SelectToken("data.gnk_company_name").ToString();
          _obj.OKEDmicros = jsonGnk.SelectToken("data.gnk_company_oked").ToString();
          _obj.LegalAddress = jsonGnk.SelectToken("data.gnk_company_address").ToString();
          _obj.RegCodeNDSmicros = jsonGnk.SelectToken("data.gnk_reg_code").ToString();
          _obj.BICmicros = jsonGnk.SelectToken("data.gnk_company_mfo").ToString();
          _obj.Account = jsonGnk.SelectToken("data.gnk_company_account").ToString();
        }
        
        
        
        if(!string.IsNullOrEmpty(strStat))
        {
          jsonStat = JObject.Parse(strStat);
          
          _obj.Email = jsonStat.SelectToken("data.stat_company_email").ToString();
          _obj.Phones = jsonStat.SelectToken("data.stat_company_phone").ToString();
        }
        
        if(!string.IsNullOrEmpty(_obj.BICmicros))
        {
          var acc = micros.DrxUzbekistan.Banks.GetAll(x => x.BIC == _obj.BICmicros).FirstOrDefault();
          if(acc != null) _obj.Bank = acc;
        }
        
        if(!string.IsNullOrEmpty(jsonGnk.SelectToken("data.gnk_company_director_name").ToString()))
          _obj.DirNamemicros = jsonGnk.SelectToken("data.gnk_company_director_name").ToString();
        
        if(!string.IsNullOrEmpty(jsonGnk.SelectToken("data.gnk_company_director_tin").ToString()))
          _obj.DirTINmicros = jsonGnk.SelectToken("data.gnk_company_director_tin").ToString();
        
      }
      catch { return false; }

      return true;
    }
  }
}