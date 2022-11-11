using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Docflow;
using Sungero.RecordManagement;
using Sungero.DirectumRX;
using Sungero;

namespace micros.AGMKModule
{
  partial class MeetingsReportClientHandlers
  {

    public override void BeforeExecute(Sungero.Reporting.Client.BeforeExecuteEventArgs e)
    {
      // Если отчёт вызывается из документа или совещания (свойство Документ или Совещание заполнено), то не показывать диалог с выбором параметров отчёта.
      if (MeetingsReport.Document != null || MeetingsReport.Meeting != null)
        return;
      
      if (MeetingsReport.BeginDate.HasValue && MeetingsReport.EndDate.HasValue &&
          MeetingsReport.ClientEndDate.HasValue)
        return;
      
      var personalSettings = Sungero.Docflow.PublicFunctions.PersonalSetting.GetPersonalSettings(null);
      var dialog = Dialogs.CreateInputDialog(Sungero.RecordManagement.Resources.ActionItemsExecutionReport);
      dialog.HelpCode = Constants.MeetingsReport.HelpCode;

      var settingsStartDate = Sungero.Docflow.PublicFunctions.PersonalSetting.GetStartDate(personalSettings);
      var beginDate = dialog.AddDate(Sungero.RecordManagement.Resources.PeriodFrom, true, settingsStartDate ?? Calendar.UserToday);
      var settingsEndDate = Sungero.Docflow.PublicFunctions.PersonalSetting.GetEndDate(personalSettings);
      var endDate = dialog.AddDate(Sungero.RecordManagement.Resources.PeriodTo, true, settingsEndDate ?? Calendar.UserToday);
      
      var author = dialog.AddSelect(Sungero.RecordManagement.Resources.AssignedBy, false, Sungero.Company.Employees.Null);
      var businessUnit = dialog.AddSelect(Sungero.Docflow.Resources.BusinessUnit, false, Sungero.Company.BusinessUnits.Null);
      var department = dialog.AddSelect(Sungero.RecordManagement.Resources.Department, false, Sungero.Company.Departments.Null);
      var performer = dialog.AddSelect(Sungero.RecordManagement.Resources.ResponsiblePerformer, false, Sungero.Company.Employees.Null);
      var meeting = dialog.AddSelect(Sungero.Meetings.Meetings.Info.LocalizedName, false, Sungero.Meetings.Meetings.Null);
      meeting.IsVisible = MeetingsReport.IsMeetingsCoverContext == true;
      
      dialog.SetOnButtonClick((args) =>
                              {
                                Sungero.Docflow.PublicFunctions.Module.CheckReportDialogPeriod(args, beginDate, endDate);
                              });
      
      dialog.Buttons.AddOkCancel();
      
      if (dialog.Show() == DialogButtons.Ok)
      {
        MeetingsReport.BeginDate = beginDate.Value.Value;
        MeetingsReport.ClientEndDate = endDate.Value.Value;
        MeetingsReport.EndDate = endDate.Value.Value.EndOfDay();
        MeetingsReport.Author = author.Value;
        MeetingsReport.BusinessUnit = businessUnit.Value;
        MeetingsReport.Department = department.Value;
        MeetingsReport.Performer = performer.Value;
        MeetingsReport.Meeting = meeting.Value;
      }
      else
      {
        e.Cancel = true;
      }
    }

  }
}