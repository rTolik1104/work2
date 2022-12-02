using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Workflow;

namespace micros.MultibankModule.Server
{
  partial class ExchangeServiceFolderHandlers
  {

    public virtual IQueryable<Sungero.Workflow.IAssignmentBase> ExchangeServiceDataQuery(IQueryable<Sungero.Workflow.IAssignmentBase> query)
    {
      return query;
    }
  }

  partial class MultibankModuleHandlers
  {
    
  }
}