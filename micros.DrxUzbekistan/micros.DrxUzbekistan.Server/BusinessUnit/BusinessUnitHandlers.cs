using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using micros.DrxUzbekistan.BusinessUnit;

namespace micros.DrxUzbekistan
{
  partial class BusinessUnitServerHandlers
  {

    public override void BeforeSave(Sungero.Domain.BeforeSaveEventArgs e)
    {
      base.BeforeSave(e);
      
      if (!string.IsNullOrEmpty(_obj.TRRC))
        _obj.TRRC = _obj.TRRC.Trim();
      
      if (!string.IsNullOrEmpty(_obj.TIN))
        _obj.TIN = _obj.TIN.Trim();
      
      if (!string.IsNullOrEmpty(_obj.PSRN))
        _obj.PSRN = _obj.PSRN.Trim();
      
      // Проверить код на пробелы, если свойство изменено.
      if (!string.IsNullOrEmpty(_obj.Code))
      {
        // При изменении кода e.AddError сбрасывается.
        var codeIsChanged = _obj.State.Properties.Code.IsChanged;
        _obj.Code = _obj.Code.Trim();
        
        if (codeIsChanged && Regex.IsMatch(_obj.Code, @"\s"))
          e.AddError(_obj.Info.Properties.Code, "Использование пробелов в коде запрещено");
      }
      
      #region Проверить корректность ИНН и дубли
      
      var result = "";
      if (_obj.TIN!=null)
      {
        if(_obj.TIN.Length < 9 || _obj.TIN.Length > 12)
          result = "Поле ИНН должно состоять из 9-12 цифр";
      }
      if (!string.IsNullOrEmpty(result))
        e.AddError(_obj.Info.Properties.TIN, result);

      if (!string.IsNullOrWhiteSpace(_obj.TIN) && _obj.Status == Sungero.CoreEntities.DatabookEntry.Status.Active)
      {
        int? companyId = null;
        if (_obj.Company != null)
          companyId = _obj.Company.Id;
        
        var warningText = Sungero.Parties.PublicFunctions.Counterparty.GetCounterpartyWithSameTinWarning(_obj.TIN, _obj.TRRC, companyId);
        
        if (!string.IsNullOrEmpty(warningText))
          e.AddWarning(warningText, _obj.Info.Actions.ShowDuplicates);
      }

      #endregion
      
      #region Проверить циклические ссылки в подчиненных НОР
      
      if (_obj.State.Properties.HeadCompany.IsChanged && _obj.HeadCompany != null)
      {
        var headCompany = _obj.HeadCompany;
        
        while (headCompany != null)
        {
          if (Equals(headCompany, _obj))
          {
            e.AddError(_obj.Info.Properties.HeadCompany, BusinessUnits.Resources.HeadCompanyCyclicReference, _obj.Info.Properties.HeadCompany);
            break;
          }
          
          headCompany = headCompany.HeadCompany;
        }
      }
      
      #endregion
    }
  }

}