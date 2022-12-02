using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace micros.Report
{
  partial class ControlReportClientHandlers
  {

    public override void BeforeExecute(Sungero.Reporting.Client.BeforeExecuteEventArgs e)
    {
      var dialog = Dialogs.CreateInputDialog("Параметры отчета");
      
      var startDate=dialog.AddDate("Начальная дата",true,Calendar.Today.AddDays(-30));
      var endDate=dialog.AddDate("Конечная дата",true,Calendar.Today.AddDays(1));
      var departments=dialog.AddSelectMany("Подразделения", true, Sungero.Company.Departments.Null);
      var registrJournal=dialog.AddSelect("Группа журналов", true, micros.Report.Groupses.Null);
      
      
      if(dialog.Show() != DialogButtons.Ok)
        e.Cancel=true;
      
      var performers = Sungero.Company.Employees.GetAll(x=>departments.Value.Contains(x.Department));
      var documentIds=new List<int>();
      foreach(var journal in registrJournal.Value.GroupRegistration)
      {
        var document=Sungero.Docflow.OfficialDocuments.GetAll(x => x.DocumentRegister.Id == journal.GroupName.Id).Select(d => d.Id);
        documentIds.AddRange(document);
      }

      foreach(var department in departments.Value)
      {
        var performersId = Sungero.Company.Employees.GetAll(x => x.Department.Id == department.Id).Select(x => x.Id);
        var assigments = Sungero.Workflow.Assignments.GetAll(x => performersId.Contains(x.Performer.Id));
        var assignments1 = assigments.Where(x => x.Created >= startDate.Value && x.Created <= endDate.Value);
        var assigments2 = assignments1.Select(t => t.Task);
        var assignmentsCount = assigments2.Cast<Sungero.Workflow.ITask>()
                                      .Where(t => t.AttachmentDetails.Any(d => documentIds.Contains(d.AttachmentId.GetValueOrDefault()))).Count();
        
        Report.Functions.Module.Remote.SetDataToDepartment(department.Name, assignmentsCount, department.Id);
      }

      foreach(var performer in performers)
      {
        var assigments = Sungero.Workflow.Assignments.GetAll(x => x.Performer.Id == performer.Id);
        
        var compltete=assigments.Where(s => s.Status.Value.ToString() == "Completed").Count();
        
        var inProcess = assigments.Where(s => s.Status.Value.ToString() == "InProcess" && s.Deadline>=Calendar.Today || s.Deadline == null)
                                 .Select(t => t.Task)
                                 .Cast<Sungero.Workflow.ITask>()
                                 .Where(t => t.AttachmentDetails.Any(d => documentIds.Contains(d.AttachmentId.GetValueOrDefault()))).Count();
        var overdue = assigments.Where(s => s.Status.Value.ToString() == "InProcess" && s.Deadline < Calendar.Today)
                                .Select(t => t.Task)
                                .Cast<Sungero.Workflow.ITask>()
                                .Where(t => t.AttachmentDetails.Any(d => documentIds.Contains(d.AttachmentId.GetValueOrDefault()))).Count();
        var wellDone = assigments.Where(s => s.Status.Value.ToString() == "Completed" && s.Completed <= s.Deadline)
                                .Select(t => t.Task)
                                .Cast<Sungero.Workflow.ITask>()
                                .Where(t => t.AttachmentDetails.Any(d => documentIds.Contains(d.AttachmentId.GetValueOrDefault()))).Count();
        var badDone = assigments.Where(s => s.Status.Value.ToString() == "Completed" && s.Completed>s.Deadline)
                                .Select(t => t.Task)
                                .Cast<Sungero.Workflow.ITask>()
                                .Where(t => t.AttachmentDetails.Any(d => documentIds.Contains(d.AttachmentId.GetValueOrDefault()))).Count();
        
        Report.Functions.Module.Remote.SetDataToTasks(performer.Name, performer.Department.Name, compltete, overdue, wellDone, badDone, inProcess, performer.Department.Id);
      }
        
      ControlReport.StartDate=startDate.Value.Value;
      ControlReport.EndDate=endDate.Value.Value;
      ControlReport.RegistrJournal = registrJournal.Value;
    }
  }
}