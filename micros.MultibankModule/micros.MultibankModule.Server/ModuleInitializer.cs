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

//    public override bool IsModuleVisible()
//    {
//      return Users.Current.IncludedIn(Roles.Administrators);
//    }

    public override void Initializing(Sungero.Domain.ModuleInitializingEventArgs e)
    {
      MultibankModule.SpecialFolders.ExchangeService.AccessRights.Grant(Roles.AllUsers, DefaultAccessRightsTypes.Change);
      MultibankModule.SpecialFolders.ExchangeService.AccessRights.Save();
      
      CreateDocumentTypes();
      //CreateDocumentKinds();
    }
    
    public static void CreateDocumentTypes()
    {
      Sungero.Docflow.PublicInitializationFunctions.Module.CreateDocumentType("Электронный договор", EContract.ClassTypeGuid, Sungero.Docflow.DocumentType.DocumentFlow.Contracts, true);
    }
    
    public static void CreateDocumentKinds()
    {
      Sungero.Docflow.PublicInitializationFunctions.Module.CreateDocumentKind("Электронный договор", "Электронный договор",
                                                                              Sungero.Docflow.DocumentKind.NumberingType.Registrable,
                                                                              Sungero.Docflow.DocumentType.DocumentFlow.Contracts, false, false,
                                                                              EContract.ClassTypeGuid,
                                                                              new Sungero.Domain.Shared.IActionInfo[] { Sungero.Docflow.OfficialDocuments.Info.Actions.SendForApproval },
                                                                              Constants.Module.ElectronicContract);
    }
  }

}
