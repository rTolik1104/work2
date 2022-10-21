using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Domain.Initialization;

namespace Demo422.QRCodeSol.Server
{
  public partial class ModuleInitializer
  {

    public override void Initializing(Sungero.Domain.ModuleInitializingEventArgs e)
    {
      CreateTableQRCodeData();
      CreateTableEImzo();
      InsertData();
      AddColumnToTable();
    }
    
    public static void CreateTableQRCodeData(){
      InitializationLogger.DebugFormat("Init: Create table qr_code_data");
      using(var command=SQL.GetCurrentConnection().CreateCommand())
      {
        command.CommandText=string.Format(Queries.Module.CreateTable);
        command.ExecuteNonQuery();
      }
    }
    
    public static void AddColumnToTable(){
      InitializationLogger.DebugFormat("Init: Add column to table");
      using(var command=SQL.GetCurrentConnection().CreateCommand())
      {
        command.CommandText=string.Format(Queries.Module.AddPasswordColumnToTable);
        command.ExecuteNonQuery();
      }
    }
    
    public static void InsertData()
    {
      var isExists=0;
      using(var command=SQL.GetCurrentConnection().CreateCommand())
      {
        command.CommandText=string.Format(Queries.Module.CheckTable);
        var obj=command.ExecuteScalar();
        isExists=Convert.ToInt32(obj);
      }
      
      if(isExists==0)
      {
        using(var command=SQL.GetCurrentConnection().CreateCommand())
        {
          command.CommandText=string.Format(Queries.Module.SetFalaseToQRCodeData);
          command.ExecuteNonQuery();
        }
      }
    }

    /// <summary>
    /// 
    /// </summary>
    public void CreateTableEImzo()
    {
      InitializationLogger.DebugFormat("Init: Create table eimzo_data");
      using(var command=SQL.GetCurrentConnection().CreateCommand())
      {
        command.CommandText=string.Format(Queries.Module.CreateTableEIMZO);
        command.ExecuteNonQuery();
      }
    }
  }
}
