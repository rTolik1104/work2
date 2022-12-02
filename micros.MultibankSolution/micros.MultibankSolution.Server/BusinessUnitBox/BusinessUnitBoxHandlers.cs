using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using System.Text;
using micros.MultibankSolution.BusinessUnitBox;

namespace micros.MultibankSolution
{
  partial class BusinessUnitBoxMultibankCompanymicrosPropertyFilteringServerHandler<T>
  {

    public virtual IQueryable<T> MultibankCompanymicrosFiltering(IQueryable<T> query, Sungero.Domain.PropertyFilteringEventArgs e)
    {
      var profileList = query.Where(x => x.Login == _obj.Login);
      return profileList;
    }
  }

  partial class BusinessUnitBoxServerHandlers
  {

    public override void Created(Sungero.Domain.CreatedEventArgs e)
    {
      base.Created(e);
      _obj.CurrenServerAddressmicros = Sungero.Domain.HelpSettings.Address.AbsoluteUri.Replace("Client/WebHelp", "");
    }

    public override void AfterSave(Sungero.Domain.AfterSaveEventArgs e)
    {
      //base.AfterSave(e);
    }

    public override void BeforeSave(Sungero.Domain.BeforeSaveEventArgs e)
    {
      if(_obj.MultibankCompanymicros != null)
      {
        string response = MultibankModule.PublicFunctions.Module.JoinProfile(Encoding.Default.GetString(_obj.AccessTokenmicros), _obj.MultibankCompanymicros.ProfileID, _obj.BusinessUnit);
        if (response.Contains("\"success\":true")) _obj.ConnectionStatus = MultibankSolution.BusinessUnitBox.ConnectionStatus.Connected;
      }

      else
        base.BeforeSave(e);
    }
  }
}