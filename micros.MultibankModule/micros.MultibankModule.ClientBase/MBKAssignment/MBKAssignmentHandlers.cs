using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using micros.MultibankModule.MBKAssignment;

namespace micros.MultibankModule
{
  partial class MBKAssignmentClientHandlers
  {

    public override void Refresh(Sungero.Presentation.FormRefreshEventArgs e)
    {
      var document = _obj.DocumentGroup.OfficialDocuments.SingleOrDefault();
      string documentStatus = document.LifeCycleState.Value.Value.Substring(0, 10).ToLower();
      var statuses = _obj.DocumentStatusAllowedItems.ToList();
      var status = statuses.Where(x => x.Value.ToLower().Contains(documentStatus)).FirstOrDefault();
      _obj.DocumentStatus = status;
      _obj.Save();
    }

    public override void Showing(Sungero.Presentation.FormShowingEventArgs e)
    {
      _obj.State.Panels.AttachmentsPreviewPanel.Activate();
      e.HideAction(_obj.Info.Actions.Annul);
      e.HideAction(_obj.Info.Actions.UpdateFromMultibank);
      if (_obj.MainTask.IntegrationDatabook.IsIncoming.Value) e.HideAction(_obj.Info.Actions.Annul);
      else
      {
        e.HideAction(_obj.Info.Actions.Accept);
        e.HideAction(_obj.Info.Actions.Reject);
      }
    }
  }
}