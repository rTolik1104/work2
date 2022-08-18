using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using System.Text;
using micros.MultibankSolution.BusinessUnitBox;

namespace micros.MultibankSolution
{
  partial class BusinessUnitBoxServerHandlers
  {

    public override void AfterSave(Sungero.Domain.AfterSaveEventArgs e)
    {
      base.AfterSave(e);
    }

    public override void BeforeSave(Sungero.Domain.BeforeSaveEventArgs e)
    {
      if(_obj.MultibankCompanymicros != null)
        MultibankModule.PublicFunctions.Module.JoinProfile(Encoding.Default.GetString(_obj.AccessTokenmicros), _obj.MultibankCompanymicros.ProfileID);
      base.BeforeSave(e);
    }
  }
}