using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using micros.MultibankModule.MBKAssignment;

namespace micros.MultibankModule.Server
{
  partial class MBKAssignmentFunctions
  {

    /// <summary>
    /// 
    /// </summary>       
    [Remote]
    public StateView GetDocumentSummary()
    {
      var document = _obj.DocumentGroup.OfficialDocuments.FirstOrDefault();
      return Sungero.Docflow.PublicFunctions.Module.GetDocumentSummary(document);
    }
  }
}