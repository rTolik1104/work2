using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using micros.DrxUzbekistan.Company;

namespace micros.DrxUzbekistan
{
  partial class CompanyClientHandlers
  {

    public virtual void BICmicrosValueInput(Sungero.Presentation.StringValueInputEventArgs e)
    {
//      var BanksList = Functions.BusinessUnit.Remote.GetAllBanks();
//			var bank = BanksList.Where(b=>b.BIC==e.NewValue).FirstOrDefault();
//			
//			if (bank != null)
//				_obj.Bank = (DirectumRxMicros.IBank)bank;
//			else
//				e.AddError("Банк с таким МФО / SWIFT не существует.");
    }

    public override void Showing(Sungero.Presentation.FormShowingEventArgs e)
    {
      base.Showing(e);
      
      if(_obj.Nonresident == true)
        micros.DrxUzbekistan.Functions.Company.NoresidentTrue(_obj);
      else if(_obj.Nonresident == false)
        micros.DrxUzbekistan.Functions.Company.NoresidentFalse(_obj);
        
    }

    public override void NonresidentValueInput(Sungero.Presentation.BooleanValueInputEventArgs e)
    {
      base.NonresidentValueInput(e);
     
      if(e.NewValue == true)
        micros.DrxUzbekistan.Functions.Company.NoresidentTrue(_obj);
      else if(e.NewValue == false)
        micros.DrxUzbekistan.Functions.Company.NoresidentFalse(_obj);
    }

    public virtual void OKEDmicrosValueInput(Sungero.Presentation.StringValueInputEventArgs e)
    {
      if(e.NewValue != null)
      {
        if(!Regex.IsMatch(e.NewValue.ToString(),@"(^\d{5}$)"))
          e.AddError("Поле ОКЭД должно состоять из 5 цифр");
      }
    }
  }


}