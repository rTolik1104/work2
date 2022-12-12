using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace micros.Report.Server
{
  public class ModuleFunctions
  {
    private void SetDataToTasks(string department_name, int all_tasks,int overdue,int well_done,int bad_done,int in_process, int stoped){
      department_name = department_name.Replace("'","''");
      using (var command = SQL.GetCurrentConnection().CreateCommand())
      {
        command.CommandText = string.Format(Queries.Module.SetDataToTasksTable, department_name, all_tasks, overdue, well_done, bad_done, in_process, stoped);
        command.ExecuteNonQuery(); 
      }
    }
    
    [Public,Remote]
    public void SetDataInTasks(List<Sungero.Company.IDepartment> departments, Report.IGroups registrJournal, DateTime? startDate, DateTime? endDate, Sungero.Company.IBusinessUnit bussnesUnit)
    {
      //Check the departmens list if is empty get all departments
      if(!departments.Any())
        departments=Sungero.Company.Departments.GetAll(x=>x.BusinessUnit.Id==bussnesUnit.Id && x.HeadOffice == null).ToList();
      
      
      //Get all documents registreted in this register
      var attachmentIds=new List<int>();
      foreach(var journal in registrJournal.GroupRegistration)
      {
        var attachment=Sungero.Docflow.OfficialDocuments.GetAll(x => x.DocumentRegister.Id == journal.GroupName.Id).Select(d => d.Id);
        attachmentIds.AddRange(attachment);
      }
      
      //Get tasks count by department and performer
      foreach(var department in departments)
      {
        int inProcessAssigmentsCount = 0;
        int overdueAssigmentsCount = 0;
        int wellDoneAssigmentsCount = 0;
        int badDoneAssigmentsCount = 0;
        int stopedAssigmentsCount = 0;
        
        var performers = Sungero.Company.Employees.GetAll(x => x.Department.Id == department.Id 
                                                          || x.Department.HeadOffice.Id == department.Id 
                                                          || x.Department.HeadOffice.HeadOffice.Id == department.Id 
                                                          || x.Department.HeadOffice.HeadOffice.HeadOffice.Id == department.Id);
        
        //Get tasks count by performer
        foreach(var performer in performers)
        {
          var assignments = Sungero.Workflow.Assignments.GetAll(x => x.Performer.Id == performer.Id && x.Created >= startDate && x.Created <= endDate);
          
          int inProcess = assignments
            .Where(s => s.Status.Value.ToString() == "InProcess" && s.Deadline>=Calendar.Today || s.Deadline == null)
            .Select(t => t.Task)
            .Cast<Sungero.Workflow.ITask>()
            .Where(t => t.AttachmentDetails.Any(d => attachmentIds.Contains(d.AttachmentId.GetValueOrDefault()))).Count();
          inProcessAssigmentsCount += inProcess;
          
          int overdue = assignments
            .Where(s => s.Status.Value.ToString() == "InProcess" && s.Deadline < Calendar.Today)
            .Select(t => t.Task)
            .Cast<Sungero.Workflow.ITask>()
            .Where(t => t.AttachmentDetails.Any(d => attachmentIds.Contains(d.AttachmentId.GetValueOrDefault()))).Count();
          overdueAssigmentsCount += overdue;
          
          int wellDone = assignments
            .Where(s => s.Status.Value.ToString() == "Completed" && s.Completed <= s.Deadline)
            .Select(t => t.Task)
            .Cast<Sungero.Workflow.ITask>()
            .Where(t => t.AttachmentDetails.Any(d => attachmentIds.Contains(d.AttachmentId.GetValueOrDefault()))).Count();
          wellDoneAssigmentsCount += wellDone;
          
          int badDone = assignments
            .Where(s => s.Status.Value.ToString() == "Completed" && s.Completed>s.Deadline)
            .Select(t => t.Task)
            .Cast<Sungero.Workflow.ITask>()
            .Where(t => t.AttachmentDetails.Any(d => attachmentIds.Contains(d.AttachmentId.GetValueOrDefault()))).Count();
          badDoneAssigmentsCount += badDone;
          
          int stoped = assignments
            .Where(s => s.Status.Value.ToString() == "Suspended" || s.Status.Value.ToString() == "Aborted")
            .Select(t => t.Task)
            .Cast<Sungero.Workflow.ITask>()
            .Where(t => t.AttachmentDetails.Any(d => attachmentIds.Contains(d.AttachmentId.GetValueOrDefault()))).Count();
          stopedAssigmentsCount += stoped;
        }
        
        int assignmentsCount = inProcessAssigmentsCount + overdueAssigmentsCount + wellDoneAssigmentsCount + badDoneAssigmentsCount + stopedAssigmentsCount;
        
        //Set data to database
        this.SetDataToTasks(department.Name, assignmentsCount, overdueAssigmentsCount, wellDoneAssigmentsCount, badDoneAssigmentsCount, inProcessAssigmentsCount, stopedAssigmentsCount);
      }
    }
  }
}