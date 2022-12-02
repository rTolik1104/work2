using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using micros.MultibankSolution.BusinessUnitBox;

namespace micros.MultibankSolution.Client
{
  partial class BusinessUnitBoxActions
  {
    public override void CheckConnection(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      if (_obj.ExchangeService.ExchangeProvider == MultibankSolution.ExchangeService.ExchangeProvider.Multibank || _obj.ExchangeService.ExchangeProvider == MultibankSolution.ExchangeService.ExchangeProvider.MultibankSTGmicros)
      {
        if (string.IsNullOrEmpty(_obj.Password))
        {
          Dialogs.NotifyMessage(BusinessUnitBoxes.Resources.EnterPasswordToCheckConnection);
          return;
        }
        var date = DateTimeOffset.FromUnixTimeSeconds(long.Parse(_obj.ExpireAccessmicros));
        if (date <= Calendar.Now)
        {
          MultibankModule.PublicFunctions.Module.Remote.IfNeedRefreshTokens(_obj);
          Dialogs.NotifyMessage("Токен обновлен");
        }
        else
        {
          Dialogs.NotifyMessage("Токен валиден до: " + date.ToString("MM/dd/yyyy HH:mm"));
        }
      }
      else base.CheckConnection(e);
    }

    public override bool CanCheckConnection(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return base.CanCheckConnection(e);
    }

    public override void Login(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      if (_obj.ExchangeService.ExchangeProvider == micros.MultibankSolution.ExchangeService.ExchangeProvider.Multibank ||
          _obj.ExchangeService.ExchangeProvider == micros.MultibankSolution.ExchangeService.ExchangeProvider.MultibankSTGmicros)
      {
        var dialog = Dialogs.CreateInputDialog(BusinessUnitBoxes.Resources.LoginActionTitle);
        var password = dialog.AddPasswordString(_obj.Info.Properties.Password.LocalizedName, true);
        dialog.Buttons.AddOkCancel();
        dialog.Buttons.Default = DialogButtons.Ok;

        dialog.SetOnButtonClick(
          x =>
          {
            if (x.Button == DialogButtons.Ok)
            {
              //_obj.Save();
              _obj.Password = password.Value;
              var isSTG = false;
              if (_obj.ExchangeService.ExchangeProvider == micros.MultibankSolution.ExchangeService.ExchangeProvider.MultibankSTGmicros) isSTG = true;
              var respond = MultibankModule.PublicFunctions.Module.AuthWithLogin(_obj.ClientIdmicros.ToString(), _obj.ClientSecretmicros, _obj.Login, _obj.Password, isSTG);  // Запрос на получение токена
              micros.MultibankModule.PublicFunctions.Module.Remote.UpdateTokens(respond, _obj);  // Вызов метода для сохранение токенов в БД.
              if (respond.Contains("\"success\":true")) _obj.ConnectionStatus = MultibankSolution.BusinessUnitBox.ConnectionStatus.Connected;
              _obj.Save();
              MultibankModule.PublicFunctions.Module.Remote.CreateProfileList(_obj);
            }
          });
        dialog.Show();
      }
      else
      {
        base.Login(e);
      }
    }

    public override bool CanLogin(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return base.CanLogin(e);
    }

  }

}