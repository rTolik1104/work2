using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using micros.DrxUzbekistan.Company;

namespace micros.DrxUzbekistan.Client
{
  partial class CompanyActions
  {
    public override void FillFromService(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      if (string.IsNullOrWhiteSpace(_obj.TIN))
        e.AddError("Заполните ИНН");
      else
      {
        var filling = micros.DrxUzbekistan.PublicFunctions.Company.FillFromServicemicrosServer(_obj);
        if(filling == false)
          e.AddError("По данному ИНН данных не обнаружено");   
        else
          e.AddInformation("Карточка заполнена по ИНН");
      }
    }

    public override bool CanFillFromService(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return base.CanFillFromService(e);
    }

  }


}