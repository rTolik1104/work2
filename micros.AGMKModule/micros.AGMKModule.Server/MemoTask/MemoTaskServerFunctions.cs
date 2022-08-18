using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using micros.AGMKModule.MemoTask;

namespace micros.AGMKModule.Server
{
  partial class MemoTaskFunctions
  {

    /// <summary>
    /// Регламент задачи согласования
    /// </summary>
    [Remote(IsPure = true)]
    public StateView GetMemoTaskState()
    {
      var stateView = StateView.Create();
      StateBlock blockAssignment;
      
      //Получение всех зависимых заданий
      var assignments = Sungero.Workflow.Assignments.GetAll()
        .Where(a => Equals(a.Task, _obj))
        .OrderBy(y => y.Created);
      
      //Блок о действии
      Sungero.Docflow.PublicFunctions.OfficialDocument
        .AddUserActionBlock(stateView,
                            _obj.Author,
                            _obj.Started.HasValue ?
                            Sungero.Docflow.ApprovalTasks.Resources.StateViewDocumentSentForApproval :
                            Sungero.Docflow.ApprovalTasks.Resources.StateViewTaskDrawCreated,
                            _obj.Created.Value,
                            _obj,
                            string.Empty,
                            _obj.Author);
      
      // Добавить основной блок для задачи.
      var taskBlock = this.AddTaskBlock(stateView);
      
      //Добавление блока по заданию
      foreach (var assignment in assignments)
      {
        blockAssignment = AddBlockAssignment(stateView, true, taskBlock, assignment, false);
      }
      
      return stateView;
    }
    
    // <summary>
    /// Построить модель состояния.
    /// </summary>
    /// <param name="databook">Справочник.</param>
    /// <returns>Схема модели состояния.</returns>
    public Sungero.Core.StateView GetStateViewMemoTask(Sungero.Docflow.IMemo memo)
    {
      return this.GetMemoTaskState();
    }
    
    /// <summary>
    /// Отображение регламента для документов
    /// </summary>
    /// <param name="document">Сам документ</param>
    /// <returns></returns>
    [Public]
    public Sungero.Core.StateView GetStateView(Sungero.Docflow.IMemo document)
    {
      return this.GetStateViewMemoTask(document);
    }
    
    
    /// <summary>
    /// Добавить основной блок задачи согласования.
    /// </summary>
    /// <param name="stateView">Схема представления.</param>
    /// <returns>Добавленный блок.</returns>
    private StateBlock AddTaskBlock(StateView stateView)
    {
      var taskBlock = stateView.AddBlock();
      
      var isDraft = _obj.Status == Sungero.Workflow.Task.Status.Draft;
      var headerStyle = Sungero.Docflow.PublicFunctions.Module.CreateHeaderStyle(isDraft);
      var labelStyle = Sungero.Docflow.PublicFunctions.Module.CreateStyle(false, isDraft, false);
      
      taskBlock.Entity = _obj;
      taskBlock.AssignIcon(Sungero.Docflow.OfficialDocuments.Info.Actions.SendForFreeApproval, StateBlockIconSize.Large);
      taskBlock.IsExpanded = _obj.Status == Sungero.Workflow.Task.Status.InProcess;
      taskBlock.AddLabel(_obj.Subject, headerStyle);
      
      var status = string.Empty;
      if (_obj.Status == Sungero.Workflow.Task.Status.InProcess)
        status = Sungero.Docflow.ApprovalTasks.Resources.StateViewInProcess;
      else if (_obj.Status == Sungero.Workflow.Task.Status.Completed)
        status = Sungero.Docflow.ApprovalTasks.Resources.StateViewCompleted;
      else if (_obj.Status == Sungero.Workflow.Task.Status.Aborted)
        status = Sungero.Docflow.ApprovalTasks.Resources.StateViewAborted;
      else if (_obj.Status == Sungero.Workflow.Task.Status.Suspended)
        status = Sungero.Docflow.ApprovalTasks.Resources.StateViewSuspended;
      else if (_obj.Status == Sungero.Workflow.Task.Status.Draft)
        status = Sungero.Docflow.ApprovalTasks.Resources.StateViewDraft;
      
      var taskText = _obj.ActiveText;
      if (taskText != string.Empty)
      {
        taskBlock.AddLineBreak();
        taskBlock.AddLabel("_______________________", labelStyle);
        taskBlock.AddLineBreak();
        taskBlock.AddLabel(taskText);
      }
      
      Sungero.Docflow.PublicFunctions.Module.AddInfoToRightContent(taskBlock, status, labelStyle);
      
      return taskBlock;
    }
    
    
    /// <summary>
    /// Добавление блока
    /// </summary>
    /// <param name="stateView">Модель</param>
    /// <param name="isShowBorder">Линия блока</param>
    /// <param name="mainBlock">Главный блок</param>
    /// <param name="assignment">Сущность (задание)</param>
    /// <param name="isExpanded">Признак того, что дочерние блоки развернуты</param>
    /// <returns></returns>
    [Public]
    public static Sungero.Core.StateBlock AddBlockAssignment(StateView stateView, bool isShowBorder, Sungero.Core.StateBlock mainBlock, Sungero.Workflow.IAssignment assignment, bool isExpanded)
    {
      StateBlockLabelStyle style = StateBlockLabelStyle.Create();
      StateBlockLabelStyle styleHeader = StateBlockLabelStyle.Create();
      StateBlockLabelStyle styleBoottom = StateBlockLabelStyle.Create();
      styleBoottom.Color = Colors.Common.Gray;
      StateBlock block;
      
      if (mainBlock != null)
        block = mainBlock.AddChildBlock();
      else
        block = stateView.AddBlock();
      
      block.AddLabel(assignment.Subject, styleHeader);
      
      string position = Sungero.Company.Employees.Is(assignment.Performer) &&
        Sungero.Company.Employees.As(assignment.Performer).JobTitle != null ?
        " (" + Sungero.Company.Employees.As(assignment.Performer).JobTitle.Name + ")":
        string.Empty;
      
      string label = string.Format("{0, -1}{1}", assignment.Performer.Name, position);
      
      //if (assignment.Deadline.HasValue)
      //label = SC.VariousProcessesModule.ApprovalBookingRequestTasks.Resources.DeadlineFormat(label, assignment.Deadline.Value.ToShortDateString());
      
      //Создано
      if (assignment.Created.HasValue)
      {
        block.AddLineBreak();
        //block.AddLabel(SC.VariousProcessesModule.ApprovalBookingRequestTasks.Resources.CreatedFormat(assignment.Created.Value), styleBoottom);
      }
      //Участник с должностью
      block.AddLineBreak();
      //block.AddLabel(SC.VariousProcessesModule.ApprovalBookingRequestTasks.Resources.ToWhomFormat(label), styleBoottom);
      
      block.ShowBorder = isShowBorder;
      block.Entity = assignment;
      block.IsExpanded = isExpanded;
      
      if (assignment.Status == Sungero.Workflow.Task.Status.InProcess)
      {
        style.Color = Colors.Common.Gray;
      }
      
      if (assignment.ActiveText != null)
      {
        block.AddLineBreak();
        //.AddLabel(SC.VariousProcessesModule.ApprovalBookingRequestTasks.Resources.CommentFormat(assignment.ActiveText), style);
      }
      
      if (assignment.Status == Sungero.Workflow.Task.Status.InProcess)
      {
        block.BorderColor = Colors.Common.DarkBlue;
      }
      if (assignment.Status == Sungero.Workflow.Task.Status.InProcess &&
          assignment.Deadline.HasValue &&
          assignment.Deadline < Calendar.Now)
      {
        style.Color = Colors.Common.Red;
        styleBoottom.Color = Colors.Common.Red;
        styleHeader.Color = Colors.Common.Red;
        block.Background = Colors.FromRgb(255, 200, 200);
      }
      
      //Статус
      var column = block.AddContent();
      if (assignment.Result != null)
        column.AddLabel(assignment.Info.Properties.Result.GetLocalizedValue(assignment.Result), style);
      else
        column.AddLabel(assignment.Info.Properties.Status.GetLocalizedValue(assignment.Status.Value), style);
      
      block.AssignIcon(StateBlockIconType.OfEntity, StateBlockIconSize.Large);
      
      return block;
    }
    
    /// <summary>
    /// Отправить проект резолюции на исполнение.
    /// </summary>
    /// <param name="parentAssignment">Задание на рассмотрение.</param>
    [Public]
    public void StartActionItemsForDraftResolution(Sungero.Workflow.IAssignment parentAssignment)
    {
      parentAssignment.Save();
      // TODO Shklyaev: переделать метод, когда сделают 65004.
      foreach (var draftResolution in _obj.ResolutionGroup.ActionItemExecutionTasks.Where(t => t.Status == Sungero.RecordManagement.ActionItemExecutionTask.Status.Draft))
      {
        // Очистить все вложения и заполнить заново, чтобы корректно отработала синхронизация вновь добавленных документов.
        var officialDocuments = draftResolution.DocumentsGroup.OfficialDocuments.ToList();
        draftResolution.DocumentsGroup.OfficialDocuments.Clear();
        
        var addendaDocuments = draftResolution.AddendaGroup.OfficialDocuments.ToList();
        draftResolution.AddendaGroup.OfficialDocuments.Clear();
        
        var othersGroup = draftResolution.OtherGroup.All.ToList();
        draftResolution.OtherGroup.All.Clear();
        
        ((Sungero.Workflow.IInternalTask)draftResolution).ParentAssignment = parentAssignment;
        ((Sungero.Workflow.IInternalTask)draftResolution).MainTask = parentAssignment.MainTask;
        draftResolution.Save();
        
        foreach (var attachment in othersGroup)
        {
          draftResolution.OtherGroup.All.Add(attachment);
          
          var participants = Sungero.Docflow.PublicFunctions.Module.Remote.GetTaskAssignees(draftResolution).ToList();
          foreach (var participant in participants)
            attachment.AccessRights.Grant(participant, DefaultAccessRightsTypes.Read);
          attachment.AccessRights.Save();
        }
        
        foreach (var attachment in officialDocuments)
          draftResolution.DocumentsGroup.OfficialDocuments.Add(attachment);
        
        foreach (var attachment in addendaDocuments)
          draftResolution.AddendaGroup.OfficialDocuments.Add(attachment);
        
        draftResolution.Save();
        ((Sungero.Domain.Shared.IExtendedEntity)draftResolution).Params[Sungero.RecordManagement.PublicConstants.ActionItemExecutionTask.CheckDeadline] = true;
        draftResolution.Start();
      }
    }
    
    /// <summary>
    /// Отправить проект резолюции на исполнение.
    /// </summary>
    /// <param name="parentAssignment">Задание на рассмотрение.</param>
    [Public]
    public void StartActionItemsForDraftResolutionSimple(Sungero.Workflow.IAssignment parentAssignment)
    {
      parentAssignment.Save();
      var assistent = Sungero.Company.ManagersAssistants.GetAll().Where(x => x.Manager == parentAssignment.Performer && x.PreparesResolution == true).FirstOrDefault();
      // TODO Shklyaev: переделать метод, когда сделают 65004.
      var dratResolutions = _obj.ResolutionGroup.ActionItemExecutionTasks.Where(t => t.Status == Sungero.RecordManagement.ActionItemExecutionTask.Status.Draft);
      foreach (var draftResolution in dratResolutions)
      {
        if (draftResolution.AssignedBy == assistent || draftResolution.AssignedBy == parentAssignment.Performer)
        {
          // Очистить все вложения и заполнить заново, чтобы корректно отработала синхронизация вновь добавленных документов.
          var officialDocuments = draftResolution.DocumentsGroup.OfficialDocuments.ToList();
          draftResolution.DocumentsGroup.OfficialDocuments.Clear();
          
          var addendaDocuments = draftResolution.AddendaGroup.OfficialDocuments.ToList();
          draftResolution.AddendaGroup.OfficialDocuments.Clear();
          
          var othersGroup = draftResolution.OtherGroup.All.ToList();
          draftResolution.OtherGroup.All.Clear();
          
          ((Sungero.Workflow.IInternalTask)draftResolution).ParentAssignment = parentAssignment;
          ((Sungero.Workflow.IInternalTask)draftResolution).MainTask = parentAssignment.MainTask;
          draftResolution.Save();
          
          foreach (var attachment in othersGroup)
          {
            draftResolution.OtherGroup.All.Add(attachment);
            
            var participants = Sungero.Docflow.PublicFunctions.Module.Remote.GetTaskAssignees(draftResolution).ToList();
            foreach (var participant in participants)
              attachment.AccessRights.Grant(participant, DefaultAccessRightsTypes.Read);
            attachment.AccessRights.Save();
          }
          
          foreach (var attachment in officialDocuments)
            draftResolution.DocumentsGroup.OfficialDocuments.Add(attachment);
          
          foreach (var attachment in addendaDocuments)
            draftResolution.AddendaGroup.OfficialDocuments.Add(attachment);

          
          draftResolution.Save();
          ((Sungero.Domain.Shared.IExtendedEntity)draftResolution).Params[Sungero.RecordManagement.PublicConstants.ActionItemExecutionTask.CheckDeadline] = true;
          draftResolution.Start();
        }
      }
    }
    
    /// <summary>
    /// Выдать права на документы исполнителям поручений
    /// </summary>
    /// <param name="task"></param>
    [Public, Remote]
    public void GiveRightsToAttachments()
    {
      var task = _obj;
      foreach(var attachment in task.DocumentGroup.OfficialDocuments)
      {
        var actionTasks = task.ResolutionGroup.ActionItemExecutionTasks;
        foreach (var atask in actionTasks)
        {
          if (atask.Assignee != null) attachment.AccessRights.Grant(atask.Assignee, DefaultAccessRightsTypes.Read);
          else
          {
            foreach (var performer in atask.ActionItemParts)
              attachment.AccessRights.Grant(performer.Assignee, DefaultAccessRightsTypes.Read);
          }
        }
      }
      
      foreach(var attachment in task.AddendaGroup.OfficialDocuments)
      {
        var actionTasks = task.ResolutionGroup.ActionItemExecutionTasks;
        foreach (var atask in actionTasks)
        {
          if (atask.Assignee != null) attachment.AccessRights.Grant(atask.Assignee, DefaultAccessRightsTypes.Read);
          else
          {
            foreach (var performer in atask.ActionItemParts)
              attachment.AccessRights.Grant(performer.Assignee, DefaultAccessRightsTypes.Read);
          }
        }
      }
    }

  }
}