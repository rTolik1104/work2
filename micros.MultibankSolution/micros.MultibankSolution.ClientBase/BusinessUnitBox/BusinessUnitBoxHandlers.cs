using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using micros.MultibankSolution.BusinessUnitBox;

namespace micros.MultibankSolution
{
  partial class BusinessUnitBoxClientHandlers
  {

    public override void ExchangeServiceValueInput(Sungero.ExchangeCore.Client.BusinessUnitBoxExchangeServiceValueInputEventArgs e)
    {
      base.ExchangeServiceValueInput(e);
      if (e.NewValue != null)
      {
        if (e.NewValue.ExchangeProvider == micros.MultibankSolution.ExchangeService.ExchangeProvider.Multibank ||
            e.NewValue.ExchangeProvider == micros.MultibankSolution.ExchangeService.ExchangeProvider.MultibankSTGmicros)
        {
          this.EnableMultibankProporties();
        }
        else this.DisableMultibankProporties();
      }
      else
      {
        this.DisableMultibankProporties();
      }
    }

    public override void Refresh(Sungero.Presentation.FormRefreshEventArgs e)
    {
      base.Refresh(e);
      if (_obj.ExchangeService != null)
      {
        if (_obj.ExchangeService.ExchangeProvider == micros.MultibankSolution.ExchangeService.ExchangeProvider.Multibank ||
            _obj.ExchangeService.ExchangeProvider == micros.MultibankSolution.ExchangeService.ExchangeProvider.MultibankSTGmicros)
        {
          this.EnableMultibankProporties();
        }
        else this.DisableMultibankProporties();
      }
      else
      {
        this.DisableMultibankProporties();
      }
    }
    
    public virtual void DisableMultibankProporties()
    {
      _obj.State.Properties.ClientIdmicros.IsVisible = false;
      _obj.State.Properties.ClientSecretmicros.IsVisible = false;
      _obj.State.Properties.MultibankCompanymicros.IsVisible = false;
      
      _obj.State.Properties.ClientIdmicros.IsRequired = false;
      _obj.State.Properties.ClientSecretmicros.IsRequired = false;
      //_obj.State.Properties.MultibankCompanymicros.IsRequired = false;
    }
    
    public virtual void EnableMultibankProporties()
    {
      _obj.State.Properties.ClientIdmicros.IsRequired = true;
      _obj.State.Properties.ClientSecretmicros.IsRequired = true;
      //_obj.State.Properties.MultibankCompanymicros.IsRequired = true;
      
      _obj.State.Properties.ClientIdmicros.IsVisible = true;
      _obj.State.Properties.ClientSecretmicros.IsVisible = true;
      _obj.State.Properties.MultibankCompanymicros.IsVisible = true;
    }
  }
}