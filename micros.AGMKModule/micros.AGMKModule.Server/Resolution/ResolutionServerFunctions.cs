using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using micros.AGMKModule.Resolution;

namespace micros.AGMKModule.Server
{
  partial class ResolutionFunctions
  {

    /// <summary>
    /// Вызов модели из основной задачи
    /// </summary>
    [Remote]
    public StateView GetResolutionState()
    {
      var stateView = StateView.Create();
      var task = MemoTasks.As(_obj.Task);
      if (task != null)
        stateView = Functions.MemoTask.GetMemoTaskState(task);
      return stateView;
    }

  }
}