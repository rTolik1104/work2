using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using micros.MultibankModule.AuthDatabook;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace micros.MultibankModule.Server
{
  partial class AuthDatabookFunctions
  {
    /// <summary> Сохранение/обновление токенов в БД. </summary>
    [Public]
    public void UpdateTokens(string respond)
    {
      JObject json = JObject.Parse(respond);
      
      //Сохранить Токен доступа и токен обновлений в БД(свойство справочника) в виде бинарных данных, и сохранение срока действие токена в виде стринг(Формат даты Linux Time)
      _obj.AccessToken = Encoding.Default.GetBytes(json.SelectToken("access_token").ToString());
      _obj.RefreshToken = Encoding.Default.GetBytes(json.SelectToken("refresh_token").ToString());
      //JObject json2 = JsonConvert.DeserializeObject<JObject>(Encoding.UTF8.GetString(Convert.FromBase64String(json.SelectToken("access_token").ToString().Split('.')[1] + "=")));
      string access_token = json.SelectToken("access_token").ToString().Split('.')[1] + "=";
      Logger.Debug("Access_Tokent = " + access_token);
      var accessTokenByte = Convert.FromBase64String(access_token);
      string accessTokenString = Encoding.UTF8.GetString(accessTokenByte);
      JObject json2 = JsonConvert.DeserializeObject<JObject>(accessTokenString);
      _obj.ExpireAccess = json2.SelectToken("exp").ToString();
      
    }
    
    [Public]
    public void CreateProfileList()
    {
      //В случае если токен уже недействителен
      //      if(Multibank.PublicFunctions.Module.isToken(Encoding.Default.GetString(_obj.RefreshToken)) != true)
      //      {
      //        var respond = Multibank.PublicFunctions.Module.AuthWithLogin(_obj.ClientId, _obj.ClientSecret, _obj.Login, _obj.Password);  // Запрос на получение токена
      //        Multibank.PublicFunctions.AuthDatabook.UpdateTokens(_obj, respond);  // Вызов метода для сохранение токенов в БД.
      //      }
      
         JObject json = JObject.Parse(MultibankModule.PublicFunctions.Module.GetProfile(Encoding.Default.GetString(_obj.AccessToken)));
    //  JObject json = JObject.Parse(Multibank.PublicFunctions.Module.GetProfile());
      
      int count = json.SelectToken("data").Count();
      
      for(int i = 0; i <= count - 1; i++)
      {
        var book = MultibankModule.ProfilesLists.Create();
        book.Login = _obj.Login;
        book.ProfileID = json.SelectToken("data.["+ i +"].profile_id").ToString();
        book.Name = json.SelectToken("data.["+ i +"].name").ToString();
        book.UserId = json.SelectToken("data.["+ i +"].user_id").ToString();
        book.Save();
      }
    }
  }
}