using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace micros.AGMKModule.Structures.Module
{
  /// <summary>
  /// Краткая информация по исполнению поручения.
  /// </summary>
  partial class LightActiomItem
  {
    public int Id { get; set; }
    
    /// <summary>
    /// Статус поручения.
    /// </summary>
    public Sungero.Core.Enumeration? Status { get; set; }
    
    /// <summary>
    /// Дата завершения.
    /// </summary>
    public DateTime? ActualDate { get; set; }
    
    /// <summary>
    /// Срок.
    /// </summary>
    public DateTime? Deadline { get; set; }
    
    /// <summary>
    /// Автор поручения.
    /// </summary>
    public IUser Author { get; set; }
    
    /// <summary>
    /// Исполнитель.
    /// </summary>
    public Sungero.Company.IEmployee Assignee { get; set; }
    
    public string ActionItem { get; set; }
    
    public Sungero.Core.Enumeration? ExecutionState { get; set; }
    
    public List<string> CoAssigneesShortNames { get; set; }
    
    public string ActiveText {get; set; }
  }
}