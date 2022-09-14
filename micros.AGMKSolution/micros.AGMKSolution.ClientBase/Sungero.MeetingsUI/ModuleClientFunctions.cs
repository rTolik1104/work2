using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace micros.AGMKSolution.Module.MeetingsUI.Client
{
  partial class ModuleFunctions
  {
    /// <summary>
    /// Отобразить отчет по исполнению поручений по совещаниям.
    /// </summary>
    public virtual void OpenActionItemExecutionReportMicros()
    {
      var actionItemExecutionReport = micros.AGMKModule.Reports.GetMeetingsReport();
      actionItemExecutionReport.IsMeetingsCoverContext = true;
      actionItemExecutionReport.Open();
    }
  }
}