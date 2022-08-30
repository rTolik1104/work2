using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using micros.MicrosSetting.AuthInfo;

namespace micros.MicrosSetting
{
  partial class AuthInfoServerHandlers
  {

    public override void BeforeSave(Sungero.Domain.BeforeSaveEventArgs e)
    {
      var authinfo = Functions.AuthInfo.GetAllAccesses().Where(x => x.Index == 1).FirstOrDefault();
     // var authinfo = listAllAuthInfo.Where(x => x.Index == 1).FirstOrDefault();
      
      if (authinfo != null)
        e.AddError("Вы не можете создать новую карточку.");
      
      base.BeforeSave(e);
    }

    public override void Created(Sungero.Domain.CreatedEventArgs e)
    {
      _obj.Index = 1;
      _obj.Name = "Информация для авторизации в Multibank";
      
      base.Created(e);
    }
  }

}