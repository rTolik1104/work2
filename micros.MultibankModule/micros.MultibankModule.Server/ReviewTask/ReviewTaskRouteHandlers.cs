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

    public virtual void StartBlock3(Sungero.Workflow.Server.NoticeArguments e)
    {
      e.Block.Subject = "Создан новый документ";
      var performer = micros.MultibankSolution.BusinessUnitBoxes.GetAll().Where(a => a.ExchangeService.ExchangeProvider == micros.MultibankSolution.ExchangeService.ExchangeProvider.Multibank || 
                                                                                a.ExchangeService.ExchangeProvider == micros.MultibankSolution.ExchangeService.ExchangeProvider.MultibankSTGmicros)
        .Where(b => b.BusinessUnit == _obj.AttachmentGroup.OfficialDocuments.First().BusinessUnit).FirstOrDefault().Responsible;
      e.Block.Performers.Add(performer);
    }

  }
}