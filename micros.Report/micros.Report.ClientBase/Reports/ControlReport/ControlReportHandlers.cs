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
      var bussnesUnit = dialog.AddSelect("Наша организация", true, Sungero.Company.BusinessUnits.Null);
      var departments=dialog.AddSelectMany("Подразделения", false, Sungero.Company.Departments.Null);
      bussnesUnit.SetOnValueChanged((b)=>{
                                      if(b.NewValue != null)
                                        departments=departments.Where(x=>x.BusinessUnit.Id==bussnesUnit.Value.Id && x.HeadOffice == null);
                                    });
      var registrJournal=dialog.AddSelect("Группа журналов", true, micros.Report.Groupses.Null);
      
      if(dialog.Show() != DialogButtons.Ok)
        e.Cancel=true;
      
      Report.Functions.Module.Remote.SetDataInTasks(departments.Value.ToList(), registrJournal.Value, startDate.Value, endDate.Value, bussnesUnit.Value);
      
      ControlReport.StartDate=startDate.Value.Value;
      ControlReport.EndDate=endDate.Value.Value;
      ControlReport.RegistrJournal = registrJournal.Value;
      ControlReport.BussnetUnit = bussnesUnit.Value;
    }
  }
}