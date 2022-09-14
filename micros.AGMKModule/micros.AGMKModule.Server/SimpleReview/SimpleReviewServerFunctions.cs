using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using micros.AGMKModule.SimpleReview;

namespace micros.AGMKModule.Server
{
  partial class SimpleReviewFunctions
  {

    /// <summary>
    /// 
    /// </summary>
    [Remote]
    public StateView GetSimpleReviewState()
    {
      var stateView = StateView.Create();
      var task = MemoTasks.As(_obj.Task);
      if (task != null)
        stateView = Functions.MemoTask.GetMemoTaskState(task);
      return stateView;
    }

  }
}