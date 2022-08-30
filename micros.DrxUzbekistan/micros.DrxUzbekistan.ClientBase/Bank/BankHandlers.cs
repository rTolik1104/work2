using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using micros.DrxUzbekistan.Bank;

namespace micros.DrxUzbekistan
{
  partial class BankClientHandlers
  {

    public override void Showing(Sungero.Presentation.FormShowingEventArgs e)
    {
      base.Showing(e);
      
      if(_obj.Nonresident == true)
        micros.DrxUzbekistan.Functions.Bank.NoresidentTrue(_obj);
      else if(_obj.Nonresident == false)
        micros.DrxUzbekistan.Functions.Bank.NoresidentFalse(_obj);
    }

    public override void NonresidentValueInput(Sungero.Presentation.BooleanValueInputEventArgs e)
    {
     // base.NonresidentValueInput(e);
      
      if(e.NewValue == true)
        micros.DrxUzbekistan.Functions.Bank.NoresidentTrue(_obj);
      else if(e.NewValue == false)
        micros.DrxUzbekistan.Functions.Bank.NoresidentFalse(_obj);
    }

    public override void BICValueInput(Sungero.Presentation.StringValueInputEventArgs e)
    {
     // base.BICValueInput(e);
    }

  }
}