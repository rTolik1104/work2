﻿using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Domain.Initialization;

namespace micros.AGMKModule.Server
{
  public partial class ModuleInitializer
  {

    public override void Initializing(Sungero.Domain.ModuleInitializingEventArgs e)
    {
      var meetingReportTableName = Constants.MeetingsReport.SourceTableName;
      Sungero.Docflow.PublicFunctions.Module.DropReportTempTables(new[] { meetingReportTableName });
      Sungero.Docflow.PublicFunctions.Module.ExecuteSQLCommandFormat(Queries.MeetingsReport.CreateActionItemExecutionReportSourceTable, new[] { meetingReportTableName });
    }
  }
}
