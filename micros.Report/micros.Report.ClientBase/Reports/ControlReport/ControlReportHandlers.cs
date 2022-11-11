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
      
      var startDate=dialog.AddDate("Период с",true,Calendar.Today.AddDays(-180));
      var endDate=dialog.AddDate("Период до",true,Calendar.Today);
      var department=dialog.AddSelect("Подразделения", true, Sungero.Company.Departments.Null);
      var registrJournal=dialog.AddSelect("Группа журналов", true, micros.Report.Groupses.Null);
      
      if(dialog.Show() != DialogButtons.Ok)
        e.Cancel=true;
      
      ControlReport.StartDate=startDate.Value.Value;
      ControlReport.EndDate=endDate.Value.Value;
      ControlReport.Departments=department.Value;
      ControlReport.RegistrJournal = registrJournal.Value;
    }

  }
}