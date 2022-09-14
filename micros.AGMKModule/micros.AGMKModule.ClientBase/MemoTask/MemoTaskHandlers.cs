using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using micros.AGMKModule.MemoTask;

namespace micros.AGMKModule
{
  partial class MemoTaskClientHandlers
  {

    public override void Refresh(Sungero.Presentation.FormRefreshEventArgs e)
    {
      if (_obj.Addressee == null)
        _obj.State.Properties.Addressee.IsVisible = false;
      if (_obj.Addressees.Count == 0)
        _obj.State.Properties.Addressees.IsVisible = false;
      _obj.State.Properties.Subject.IsEnabled = false;
      _obj.State.Properties.ReqApprovers.IsEnabled = false;
    }

  }
}