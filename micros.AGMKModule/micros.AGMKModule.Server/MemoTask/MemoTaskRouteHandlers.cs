using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Workflow;
using micros.AGMKModule.MemoTask;

namespace micros.AGMKModule.Server
{
  partial class MemoTaskRouteHandlers
  {

    public virtual void StartBlock14(micros.AGMKModule.Server.ReworkArguments e)
    {
      e.Block.Subject = "Доработайте " + _obj.DocumentGroup.OfficialDocuments.First().Name;
      e.Block.Performers.Add(_obj.Author);
    }

    public virtual void StartBlock13(micros.AGMKModule.Server.SimpleReviewArguments e)
    {
      e.Block.Subject = "Подтвердите проект резолюции " + _obj.DocumentGroup.OfficialDocuments.First().Name;
      foreach (var adressee in _obj.Addressees.ToList())
      {
        e.Block.Performers.Add(adressee.Addressee);
        _obj.DocumentGroup.OfficialDocuments.First().AccessRights.Grant(adressee.Addressee, DefaultAccessRightsTypes.FullAccess);
      }
    }

    public virtual void StartBlock12(micros.AGMKModule.Server.ResolutionArguments e)
    {
      e.Block.Subject = "Подготовьте проект резолюции для " + _obj.DocumentGroup.OfficialDocuments.First().Name;
      foreach (var adressee in _obj.Addressees.ToList())
      {
        var performers = Sungero.Company.ManagersAssistants.GetAll().Where(x => x.Manager == adressee.Addressee).Where(x => x.PreparesResolution == true).ToList();
        foreach (var performer in performers)
        {
          e.Block.Performers.Add(performer.Assistant);
          _obj.DocumentGroup.OfficialDocuments.First().AccessRights.Grant(performer.Assistant, DefaultAccessRightsTypes.Read);
        }
      }
    }

    public virtual bool Decision11Result()
    {
      //throw new System.NotImplementedException();
      if (_obj.Addressee != null) return true;
      else return false;
    }

    public virtual void StartBlock10(micros.AGMKModule.Server.ResolutionArguments e)
    {
      
      e.Block.Subject = "Подготовьте проект резолюции для " + _obj.DocumentGroup.OfficialDocuments.First().Name;
      var performer = Sungero.Company.ManagersAssistants.GetAll().Where(x => x.Manager == _obj.Addressee).FirstOrDefault().Assistant;
      e.Block.Performers.Add(performer);
      _obj.DocumentGroup.OfficialDocuments.First().AccessRights.Grant(performer, DefaultAccessRightsTypes.Read);
    }

    public virtual bool Decision3Result()
    {
      //throw new System.NotImplementedException();
      var manager = Sungero.Company.Employees.GetAll().Where(x => x.Id == _obj.Author.Id).FirstOrDefault().Department.Manager;
      if (_obj.Addressee == manager || _obj.Addressees.Any(x => x.Addressee == manager)) return true;
      else return false;
    }

    public virtual void StartBlock9(Sungero.Workflow.Server.NoticeArguments e)
    {
      e.Block.Subject = "Уведомление о завершении рассмотрения " + _obj.DocumentGroup.OfficialDocuments.First().Name;
      e.Block.Performers.Add(_obj.Author);
    }

    public virtual void StartBlock8(micros.AGMKModule.Server.ReviewArguments e)
    {
      
      e.Block.Performers.Add(_obj.Addressee);
      _obj.DocumentGroup.OfficialDocuments.First().AccessRights.Grant(_obj.Addressee, DefaultAccessRightsTypes.FullAccess);
    }

    public virtual void StartBlock4(micros.AGMKModule.Server.ApprovalArguments e)
    {
      e.Block.Subject = "Согласуйте " + _obj.DocumentGroup.OfficialDocuments.First().Name;
      var document = _obj.DocumentGroup.OfficialDocuments.First();
      foreach (var performer in _obj.AddApprovers.ToList())
      {
        e.Block.Performers.Add(performer.Approver);
        document.AccessRights.Grant(performer.Approver, DefaultAccessRightsTypes.FullAccess);
      }
    }

    public virtual void StartBlock5(micros.AGMKModule.Server.ApprovalArguments e)
    {
      e.Block.Subject = "Согласуйте " + _obj.DocumentGroup.OfficialDocuments.First().Name;
      foreach(var performer in _obj.ReqApprovers.ToList())
      {
        e.Block.Performers.Add(performer.Approver);
        _obj.DocumentGroup.OfficialDocuments.First().AccessRights.Grant(performer.Approver, DefaultAccessRightsTypes.FullAccess);
      }
      
    }

    public virtual void StartBlock6(micros.AGMKModule.Server.SigningArguments e)
    {
      e.Block.Subject = "Подпишите " + _obj.DocumentGroup.OfficialDocuments.First().Name;
      if (Signatures.Get(_obj.DocumentGroup.OfficialDocuments.First().LastVersion).Where(z => z.SignatureType == SignatureType.Approval).Any(x => x.Signatory == _obj.Signatory)) e.Block.Performers.Clear();
      else
      {
        e.Block.Performers.Add(_obj.Signatory);
        _obj.DocumentGroup.OfficialDocuments.First().AccessRights.Grant(_obj.Signatory, DefaultAccessRightsTypes.FullAccess);
      }

    }

  }
}