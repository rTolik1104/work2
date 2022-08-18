using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using micros.MultibankModule.AuthDatabook;

namespace micros.MultibankModule.Client
{
  partial class AuthDatabookActions
  {
    public override void Save(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      base.Save(e);
    }

    public override bool CanSave(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return base.CanSave(e);
    }

    public virtual void GetProfiles(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      MultibankModule.PublicFunctions.AuthDatabook.CreateProfileList(_obj);
    }

    public virtual bool CanGetProfiles(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return true;
    }

  }

}