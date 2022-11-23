using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace micros.Report
{
  partial class ControlReportServerHandlers
  {

    public override void BeforeExecute(Sungero.Reporting.Server.BeforeExecuteEventArgs e)
    {
      
    }
    
    public override void AfterExecute(Sungero.Reporting.Server.AfterExecuteEventArgs e)
    {

      using (var command = SQL.GetCurrentConnection().CreateCommand())
      {
        command.CommandText = string.Format(Queries.Module.ClearDepartmentsTable);
        command.ExecuteNonQuery(); 
      }

      using (var command = SQL.GetCurrentConnection().CreateCommand())
      {
        command.CommandText = string.Format(Queries.Module.ClearTasksTable);
        command.ExecuteNonQuery(); 
      }
    }
  }

}