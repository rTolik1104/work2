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
    private static string GetMeetingsText(IEnumerable<Sungero.Meetings.IMeeting> meetings)
    {
      return string.Join("; ", meetings.Select(x => x.DisplayName));
    }
    
    public override void BeforeExecute(Sungero.Reporting.Client.BeforeExecuteEventArgs e)
    {
      // Если отчёт вызывается из документа или совещания (свойство Документ или Совещание заполнено), то не показывать диалог с выбором параметров отчёта.
      if (MeetingsReport.Document != null || !string.IsNullOrEmpty(MeetingsReport.Meetings))
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
      var meetings = dialog.AddSelectMany(micros.AGMKModule.Reports.Resources.MeetingsReport.Meetings, false, Sungero.Meetings.Meetings.Null);
        meetings.IsEnabled = false;
        meetings.IsVisible = false;
        var meetingsText = dialog
          .AddMultilineString(micros.AGMKModule.Reports.Resources.MeetingsReport.Meetings, false, GetMeetingsText(meetings.Value))
          .RowsCount(3);
        meetingsText.IsEnabled = false;
        var addMeetings = dialog.AddHyperlink(micros.AGMKModule.Reports.Resources.MeetingsReport.AddMeetings);
        var deleteMeetings = dialog.AddHyperlink(micros.AGMKModule.Reports.Resources.MeetingsReport.DeleteMeetings);
      meetingsText.IsVisible = MeetingsReport.IsMeetingsCoverContext == true;
      addMeetings.IsVisible = MeetingsReport.IsMeetingsCoverContext == true;
      deleteMeetings.IsVisible = MeetingsReport.IsMeetingsCoverContext == true;
      addMeetings.IsEnabled = true;
      deleteMeetings.IsEnabled = true;
      
      dialog.SetOnButtonClick((args) =>
                              {
                                Sungero.Docflow.PublicFunctions.Module.CheckReportDialogPeriod(args, beginDate, endDate);
                              });
      
      meetings.SetOnValueChanged(
        (args) =>
        {
          meetingsText.Value = GetMeetingsText(args.NewValue);
        });
      
      addMeetings.SetOnExecute(
        () =>
        {
          var selectedMeetings = AGMKModule.Functions.Module.Remote.GetMeetings()
            .Where(ca => ca.Status == Sungero.CoreEntities.DatabookEntry.Status.Active)
            .ShowSelectMany(micros.AGMKModule.Reports.Resources.MeetingsReport.ChooseMeetingsToAdd);
          if (selectedMeetings != null && selectedMeetings.Any())
          {
            var newMeetings = new List<Sungero.Meetings.IMeeting>();
            newMeetings.AddRange(meetings.Value);
            newMeetings.AddRange(selectedMeetings);
            meetings.Value = newMeetings.Distinct();
          }
        });
      
      deleteMeetings.SetOnExecute(
        () =>
        {
          var selectedMeetings = meetings.Value.ShowSelectMany(micros.AGMKModule.Reports.Resources.MeetingsReport.ChooseMeetingsToDelete);
          if (selectedMeetings != null && selectedMeetings.Any())
          {
            var newMeetings = new List<Sungero.Meetings.IMeeting>();
            foreach (var meeting in meetings.Value)
            {
              if (!selectedMeetings.Contains(meeting))
                newMeetings.Add(meeting);
            }
            meetings.Value = newMeetings;
          }
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
        if (meetings.Value.Count() == 1)
        {
          MeetingsReport.Meetings = meetings.Value.Select(s => s.Id.ToString()).FirstOrDefault();
          MeetingsReport.Meeting = meetings.Value.FirstOrDefault();
        }
        else
          MeetingsReport.Meetings = string.Join(";", meetings.Value.Select(s => s.Id.ToString()).ToArray());
      }
      else
      {
        e.Cancel = true;
      }
    }

  }
}