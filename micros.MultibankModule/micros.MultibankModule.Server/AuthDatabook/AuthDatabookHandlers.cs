using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using micros.MultibankModule.AuthDatabook;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;

namespace micros.MultibankModule
{
  partial class AuthDatabookServerHandlers
  {

    public override void AfterSave(Sungero.Domain.AfterSaveEventArgs e)
    {
      
    }
    /// <summary> Заново получить токены при сохранений. /// </summary>
    public override void BeforeSave(Sungero.Domain.BeforeSaveEventArgs e)
    {
      var respond = MultibankModule.PublicFunctions.Module.AuthWithLogin(_obj.ClientId, _obj.ClientSecret, _obj.Login, _obj.Password, _obj.IsStaging.Value);  // Запрос на получение токена
      MultibankModule.PublicFunctions.AuthDatabook.UpdateTokens(_obj, respond);  // Вызов метода для сохранение токенов в БД.
      if(_obj.MultibankCompany != null)
        MultibankModule.PublicFunctions.Module.JoinProfile(Encoding.Default.GetString(_obj.AccessToken), _obj.MultibankCompany.ProfileID);
    }
  }

}