using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using micros.MultibankSolution.Contract;

namespace micros.MultibankSolution
{
  partial class ContractClientHandlers
  {

    public override void Refresh(Sungero.Presentation.FormRefreshEventArgs e)
    {
      base.Refresh(e);
    }

    public override void Showing(Sungero.Presentation.FormShowingEventArgs e)
    {
      base.Showing(e);
      if(!_obj.State.IsInserted)
      {
        var databook = MultibankModule.IntegrationDatabooks.GetAll().Where(x => x.Document == _obj).FirstOrDefault();
        if (databook == null)
        {
          e.HideAction(_obj.Info.Actions.UpdateFromMultibankmicros);
          e.HideAction(_obj.Info.Actions.OpenInMultibankmicros);
        }
      }
      else
      {
        e.HideAction(_obj.Info.Actions.UpdateFromMultibankmicros);
        e.HideAction(_obj.Info.Actions.OpenInMultibankmicros);
      }
    }
  }
}