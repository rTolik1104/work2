﻿using System;
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
      
      var startDate=dialog.AddDate("Начальная дата",true,Calendar.Today.AddDays(-180));
      var endDate=dialog.AddDate("Конечная дата",true,Calendar.Today);
      var departments=dialog.AddSelectMany("Подразделения", true, Sungero.Company.Departments.Null);
      var registrJournal=dialog.AddSelect("Группа журналов", true, micros.Report.Groupses.Null);
      
      
      if(dialog.Show() != DialogButtons.Ok)
        e.Cancel=true;
      
      var performers = Sungero.Company.Employees.GetAll(x=>departments.Value.Contains(x.Department));
      var documents=new List<int>();
      foreach(var journal in registrJournal.Value.GroupRegistration)
      {
        var document=Sungero.Docflow.OfficialDocuments.GetAll(x=>x.DocumentRegister==journal.GroupName).Select(d=>d.Id);
        documents.AddRange(document);
      }

      foreach(var department in departments.Value)
      {
        var performersId = Sungero.Company.Employees.GetAll(x=>x.Department==department).Select(x=>x.Id);
        var assignmentsCount = Sungero.Workflow.Assignments.GetAll(x => performersId.Contains(x.Performer.Id) && x.Created>=startDate.Value && x.Created<=endDate.Value && documents.Contains(x.Attachments.Select(a=>a.Id).First())).Count();
        Report.Functions.Module.Remote.SetDataToDepartment(department.Name, assignmentsCount, department.Id);
      }

      foreach(var performer in performers)
      {
        var compltete=Sungero.Workflow.Assignments.GetAll(x => x.Performer==performer).Where(s=>s.Status.Value.ToString()=="Completed").Count();
        var process=Sungero.Workflow.Assignments.GetAll(x => x.Performer==performer).Where(s=>s.Status.Value.ToString()=="InProcess" && s.Deadline>=Calendar.Today && documents.Contains(s.Attachments.Select(a=>a.Id).First())).Count();
        var overd=Sungero.Workflow.Assignments.GetAll(x => x.Performer==performer).Where(s=>s.Status.Value.ToString()=="InProcess" && s.Deadline<Calendar.Today && documents.Contains(s.Attachments.Select(a=>a.Id).First())).Count();
        var well=Sungero.Workflow.Assignments.GetAll(x => x.Performer==performer).Where(s=>s.Status.Value.ToString()=="Completed" && s.Completed<=s.Deadline && documents.Contains(s.Attachments.Select(a=>a.Id).First())).Count();
        var bad=Sungero.Workflow.Assignments.GetAll(x => x.Performer==performer).Where(s=>s.Status.Value.ToString()=="Completed" && s.Completed>s.Deadline && documents.Contains(s.Attachments.Select(a=>a.Id).First())).Count();
        
        Report.Functions.Module.Remote.SetDataToTasks(performer.Name,performer.Department.Name,compltete,overd,well,bad,process, performer.Department.Id);
      }
        
      ControlReport.StartDate=startDate.Value.Value;
      ControlReport.EndDate=endDate.Value.Value;
      ControlReport.RegistrJournal = registrJournal.Value;
    }
  }
}