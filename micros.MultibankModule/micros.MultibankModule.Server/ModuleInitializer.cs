using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Domain.Initialization;

namespace micros.MultibankModule.Server
{
  public partial class ModuleInitializer
  {

    public override bool IsModuleVisible()
    {
      return Users.Current.IncludedIn(Roles.Administrators);
    }

    public override void Initializing(Sungero.Domain.ModuleInitializingEventArgs e)
    {
      MultibankModule.SpecialFolders.ExchangeService.AccessRights.Grant(Roles.AllUsers, DefaultAccessRightsTypes.Change);
      MultibankModule.SpecialFolders.ExchangeService.AccessRights.Save();
    }
  }

}
