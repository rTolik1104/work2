using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Domain.Initialization;

namespace micros.Report.Server
{
  public partial class ModuleInitializer
  {
    
    public virtual void Createdatabook()
    {
      var databooks=micros.Report.Groupses.GetAll();
      if(databooks.Count()==0)
      {
        var databook = micros.Report.Groupses.Create();
        databook.Name="Группы регистрации";
        databook.Save();
      }
    }
  }
}
