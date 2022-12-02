using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Workflow;
using micros.MultibankModule.ReviewTask;

namespace micros.MultibankModule.Server
{
  partial class ReviewTaskRouteHandlers
  {

    public virtual void CompleteAssignment4(micros.MultibankModule.IMBKAssignment assignment, micros.MultibankModule.Server.MBKAssignmentArguments e)
    {
      if(assignment.Performer != null) _obj.Performer =  Sungero.Company.Employees.As(assignment.Performer);
      _obj.Save();
    }

    public virtual void StartBlock4(micros.MultibankModule.Server.MBKAssignmentArguments e)
    {
      //MultibankModule.SpecialFolders.ForProccesing.AccessRights.Grant(_obj.Performer, DefaultAccessRightsTypes.Change);
      var document = _obj.DocumentGroup.OfficialDocuments.SingleOrDefault();
      var box = MultibankModule.PublicFunctions.Module.Remote.GetBuisnesUnitBoxForCompany(document.BusinessUnit);
      //e.Block.DocumentStatus = MultibankModule.PublicFunctions.Module.Remote.GetDocumentStatusForMBKAssignment(_obj);
      e.Block.RelativeDeadlineDays = box.DeadlineInDays;
      e.Block.RelativeDeadlineHours = box.DeadlineInHours;
      e.Block.Performers.Clear();
      e.Block.Performers.Add(_obj.Performer);
      _obj.AccessRights.Grant(_obj.Performer, DefaultAccessRightsTypes.FullAccess);
      document.AccessRights.Grant(_obj.Performer, DefaultAccessRightsTypes.Change);
      _obj.AccessRights.Save();
      document.AccessRights.Save();
      //_obj.IntegrationDatabook.AccessRights.Grant(_obj.Performer, DefaultAccessRightsTypes.FullAccess);
    }
  }

}