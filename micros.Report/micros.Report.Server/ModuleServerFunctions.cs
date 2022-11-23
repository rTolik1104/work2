using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace micros.Report.Server
{
  public class ModuleFunctions
  {
    [Public,Remote]
    public void SetDataToTasks(string emolyee_name,string department,int completed,int overdue,int well_done,int bad_done,int in_process, int department_id){
      using (var command = SQL.GetCurrentConnection().CreateCommand())
      {
        command.CommandText = string.Format(Queries.Module.SetDataToTasksTable, emolyee_name,department, completed,overdue,well_done,bad_done,in_process,department_id);
        command.ExecuteNonQuery(); 
      }
    }
    
    [Public,Remote]
    public void SetDataToDepartment(string department,int tasksCount, int department_id){
      using (var command = SQL.GetCurrentConnection().CreateCommand())
      {
        command.CommandText = string.Format(Queries.Module.SetDataToDepartmentTable, department, tasksCount,department_id);
        command.ExecuteNonQuery(); 
      }
    }
  }
}