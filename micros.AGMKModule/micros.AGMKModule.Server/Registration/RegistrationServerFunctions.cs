using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using micros.AGMKModule.Registration;

namespace micros.AGMKModule.Server
{
  partial class RegistrationFunctions
  {

    /// <summary>
    /// Вызов модели из основной задачи
    /// </summary>
    [Remote]
    public StateView GetRegistrationState()
    {
      var stateView = StateView.Create();
      var task = MemoTasks.As(_obj.Task);
      if (task != null)
        stateView = Functions.MemoTask.GetMemoTaskState(task);
      return stateView;
    }
  }

}