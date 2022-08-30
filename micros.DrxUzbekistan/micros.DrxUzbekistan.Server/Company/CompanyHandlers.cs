using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using micros.DrxUzbekistan.Company;

namespace micros.DrxUzbekistan
{
  partial class CompanyServerHandlers
  {

    public override void Saved(Sungero.Domain.SavedEventArgs e)
    {
      base.Saved(e);
      
      if(!String.IsNullOrEmpty(_obj.DirNamemicros))
      {
        var dir = Sungero.Parties.Contacts.GetAll(x => x.Name == _obj.DirNamemicros).FirstOrDefault();
        if(dir == null)
        {
          var director = Sungero.Parties.Contacts.Create();
          director.Name = _obj.DirNamemicros;
          director.Save();
        }
      }
    }
  }


}