using System;
using Sungero.Core;

namespace micros.Multibank.Constants
{
  public static class Module
  {
    #region Типы документов Multibank
    
    /// <summary>Типы документов Multibank</summary>
    public static class DoctypeId
    {
      /// <summary>Счёт-фактура новая</summary>
      public const string Invoice = "5dc512c3-bc0b-419f-9254-0fc00e2569ef";
      
      /// <summary>Товарно-транспортная накладная (ТТН)</summary>
      public const string TTH = "e6dc135c-33f8-4f49-b6f4-b3302db26b0a";
      
      /// <summary>Контракт</summary>
      public const string Contract = "6fecad32-1b27-49db-adf3-12460439f735";
      
      /// <summary>Доверенность</summary>
      public const string PowerOfAttorney = "945dde7f-b55e-4666-990c-749751a3c726";
      
      /// <summary>Акт</summary>
      public const string Actum = "42616ee4-1b6e-4015-b9c6-922a459bc170";
      
      /// <summary>Договор ГНК</summary>
      public const string GnkContract = "f0cd7256-71fe-465a-b26e-0840ca052c48";
    }
    
    #endregion
    
    #region Статусы транзакций ЭДО
    
    /// <summary>Транзакций: СЧЕТА-ФАКТУРЫ </summary>
    public static class InvoiceTransaction
    {
      /// <summary>"Документ создан поставщиком" (Общее)</summary>
      public const string CreateByOwner = "CreateByOwner";
    }
    
    #endregion
  }
}