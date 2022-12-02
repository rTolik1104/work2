using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using System.Globalization;
using Sungero.CoreEntities;
using micros.MultibankSolution.OutgoingTaxInvoice;

namespace micros.MultibankSolution.Server
{
  partial class OutgoingTaxInvoiceFunctions
  {
    /// <summary>
    /// Построить сводку по документу.
    /// </summary>
    /// <returns>Сводка по документу.</returns>
    public override StateView GetDocumentSummary()
    {
      var documentSummary = StateView.Create();
      var block = documentSummary.AddBlock();
      
      // Краткое имя документа.
      var documentName = _obj.DocumentKind.ShortName;
      if (!string.IsNullOrWhiteSpace(_obj.RegistrationNumber))
        documentName += Sungero.Docflow.OfficialDocuments.Resources.Number + _obj.RegistrationNumber;
      
      if (_obj.RegistrationDate != null)
        documentName += Sungero.Docflow.OfficialDocuments.Resources.DateFrom + _obj.RegistrationDate.Value.ToString("d");
      
      block.AddLabel(documentName);
      block.AddLineBreak();
      block.AddEmptyLine();
      
      //Наша Орг.
      block.AddLabel("Поставщик:");
      block.AddHyperlink(_obj.BusinessUnit.Name, Hyperlinks.Get(_obj.BusinessUnit));
      block.AddLineBreak();
      
      //Контрагент
      block.AddLabel("Получатель:");
      block.AddHyperlink(_obj.Counterparty.Name, Hyperlinks.Get(_obj.Counterparty));
      block.AddLineBreak();
      
      // Сумма
      block.AddLabel(string.Format("{0}: {1}", _obj.Info.Properties.TotalAmount.LocalizedName, _obj.TotalAmount.Value.ToString("N")));
      block.AddHyperlink(_obj.Currency.ShortName, Hyperlinks.Get(_obj.Currency));
      block.AddLineBreak();
      
      return documentSummary;
    }
  }
}