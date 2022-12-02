using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using micros.MultibankModule.EContract;

namespace micros.MultibankModule
{
  partial class EContractGoodsSharedHandlers
  {

    public virtual void GoodsTotalSumChanged(Sungero.Domain.Shared.DoublePropertyChangedEventArgs e)
    {
      double summ = 0;
      foreach (var good in _obj.EContract.Goods)
      {
        summ = summ + good.TotalSum.Value;
      }
      _obj.EContract.TotalAmount = summ;
    }
  }

  partial class EContractSharedHandlers
  {

    public virtual void GoodsChanged(Sungero.Domain.Shared.CollectionPropertyChangedEventArgs e)
    {
      
    }

    public override void CounterpartyChanged(Sungero.Docflow.Shared.ContractualDocumentBaseCounterpartyChangedEventArgs e)
    {
      base.CounterpartyChanged(e);
    }

  }
}