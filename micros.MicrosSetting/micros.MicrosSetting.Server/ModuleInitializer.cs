using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Domain.Initialization;

namespace micros.MicrosSetting.Server
{
  public partial class ModuleInitializer
  {

    public override void Initializing(Sungero.Domain.ModuleInitializingEventArgs e)
    {
      CreateTableForMultibank();
      InsertData();
    }
    
    public static void CreateTableForMultibank(){
      InitializationLogger.Debug("Init: Create table for multibank authorization");
      
      using(var command = SQL.GetCurrentConnection().CreateCommand())
      {
        command.CommandText=string.Format(Queries.Module.CreateTableMultibankAuthorization);
        command.ExecuteNonQuery();
      }
    }
    
    public static void InsertData()
    {
      int isExists=0;
      using(var command = SQL.GetCurrentConnection().CreateCommand())
      {
        command.CommandText=string.Format(Queries.Module.CheckTable);
        var obj= command.ExecuteScalar();
        isExists=Convert.ToInt32(obj);
      }
      if(isExists==0)
      {
        using(var command = SQL.GetCurrentConnection().CreateCommand())
        {
          command.CommandText=string.Format(Queries.Module.InsertDataToMultibank);
          command.ExecuteNonQuery();
        }
      }
    }
  }
}
