using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using micros.AGMKModule.MemoTask;

namespace micros.AGMKModule.Shared
{
  partial class MemoTaskFunctions
  {
    /// <summary>
    /// Проверить наличие документа на рассмотрение в задаче и наличие хоть каких-то прав на него.
    /// </summary>
    /// <returns>True, если с документом можно работать.</returns>
    [Public]
    public virtual bool HasDocumentAndCanRead()
    {
      return _obj.DocumentGroup.OfficialDocuments.Any();
    }
    /// <summary>
    /// Получить список просроченных задач на исполнение поручения в состоянии Черновик.
    /// </summary>
    /// <returns>Список просроченных задач на исполнение поручения в состоянии Черновик.</returns>
    public virtual List<Sungero.RecordManagement.IActionItemExecutionTask> GetDraftOverdueActionItemExecutionTasks()
    {
      var tasks = _obj.ResolutionGroup.ActionItemExecutionTasks.Where(t => t.Status == Sungero.RecordManagement.ActionItemExecutionTask.Status.Draft);
      var overdueTasks = new List<Sungero.RecordManagement.IActionItemExecutionTask>();
      foreach (var task in tasks)
        if (Sungero.RecordManagement.PublicFunctions.ActionItemExecutionTask.CheckOverdueActionItemExecutionTask(task))
          overdueTasks.Add(task);
      
      return overdueTasks;
    }
    
  }
}