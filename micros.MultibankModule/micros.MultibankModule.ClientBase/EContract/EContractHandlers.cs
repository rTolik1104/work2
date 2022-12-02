using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using micros.MultibankModule.EContract;

namespace micros.MultibankModule
{
  partial class EContractClientHandlers
  {

    public override void BusinessUnitValueInput(Sungero.Docflow.Client.OfficialDocumentBusinessUnitValueInputEventArgs e)
    {
      base.BusinessUnitValueInput(e);
      if (String.IsNullOrEmpty(e.NewValue.TIN)) e.AddError("Заполните ИНН нашей организации");
      if (e.NewValue.CEO == null) e.AddError("В нашей организации отсутствует руководитель");
      if (e.NewValue.CEO != null && String.IsNullOrEmpty(micros.DrxUzbekistan.People.As(e.NewValue.CEO.Person).PINImicros)) e.AddError("Заполните ИНН руководителя нашей организации");
      if (String.IsNullOrEmpty(e.NewValue.Phones)) e.AddError("Заполните номер нашей организации");
    }

    public override void ContactValueInput(Sungero.Contracts.Client.ContractualDocumentContactValueInputEventArgs e)
    {
      base.ContactValueInput(e);
      if (e.NewValue.Person == null) e.AddError("У контакта отсутствует персона");
      if (e.NewValue.Person != null && String.IsNullOrEmpty(micros.DrxUzbekistan.People.As(e.NewValue.Person).PINImicros)) e.AddError("Заполните ПИНФЛ контакта в персоне");
    }

    public override void CounterpartyValueInput(Sungero.Docflow.Client.ContractualDocumentBaseCounterpartyValueInputEventArgs e)
    {
      base.CounterpartyValueInput(e);
      if (String.IsNullOrEmpty(e.NewValue.TIN)) e.AddError("Заполните ИНН контрагента");
      if (String.IsNullOrEmpty(e.NewValue.Account)) e.AddError("Заполните номер счета контрагента");
      if (e.NewValue.Bank == null) e.AddError("Заполните банк в контрагенте");
      if (e.NewValue.Bank != null)
        if (String.IsNullOrEmpty(e.NewValue.Bank.BIC)) e.AddError("Заполните МФО банка в контрагенте");
    }
  }
}