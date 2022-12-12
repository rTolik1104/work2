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

    public override void Initializing(Sungero.Domain.ModuleInitializingEventArgs e)
    {
      CreateTableForTasks();
    }
    
    public static void CreateTableForTasks(){
      InitializationLogger.DebugFormat("Init: Create table micros_report_aloqabank");
      using(var command=SQL.GetCurrentConnection().CreateCommand())
      {
        command.CommandText=string.Format(Queries.Module.CreateReportTableForTasksCount);
        command.ExecuteNonQuery();
      }
    }
  }
}
