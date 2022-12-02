using System;
using System.Globalization;
using System.Text;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using micros.MultibankSolution.ContractStatement;

namespace micros.MultibankSolution.Server
{
  partial class ContractStatementFunctions
  {
    /// <summary>
    /// Построить сводку по документу.
    /// </summary>
    /// <returns>Сводка по документу.</returns>
    public override StateView GetDocumentSummary()
    {
      var documentSummary = StateView.Create();
      
      var databook = MultibankModule.IntegrationDatabooks.GetAll().Where(x => x.Document == _obj).FirstOrDefault();
      
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
      
      //Поставщик
      block.AddLabel("Поставщик:");
      if(databook.IsIncoming.Value) block.AddHyperlink(_obj.Counterparty.Name, Hyperlinks.Get(_obj.Counterparty));
      else block.AddHyperlink(_obj.BusinessUnit.Name, Hyperlinks.Get(_obj.BusinessUnit));
      block.AddLineBreak();
      
      //Получатель
      block.AddLabel("Получатель:");
      if(databook.IsIncoming.Value) block.AddHyperlink(_obj.BusinessUnit.Name, Hyperlinks.Get(_obj.BusinessUnit));
      else block.AddHyperlink(_obj.Counterparty.Name, Hyperlinks.Get(_obj.Counterparty));
      block.AddLineBreak();
      
      // Сумма
      double summ = _obj.TotalAmount.Value;

      block.AddLabel(string.Format("{0}: {1}", _obj.Info.Properties.TotalAmount.LocalizedName, summ.ToString("N")));
      block.AddHyperlink(_obj.Currency.ShortName, Hyperlinks.Get(_obj.Currency));
      block.AddLineBreak();
      
      return documentSummary;
    }
    
  }
}