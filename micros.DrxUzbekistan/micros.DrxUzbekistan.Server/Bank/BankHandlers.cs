using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using micros.DrxUzbekistan.Bank;

namespace micros.DrxUzbekistan
{
  partial class BankServerHandlers
  {

    public override void BeforeSave(Sungero.Domain.BeforeSaveEventArgs e)
    {
      if (!string.IsNullOrEmpty(_obj.SWIFT))
      {
        var newSwift = _obj.SWIFT.Trim();
        if (!_obj.SWIFT.Equals(newSwift, StringComparison.InvariantCultureIgnoreCase))
          _obj.SWIFT = newSwift;
      }
      
      if (!string.IsNullOrEmpty(_obj.BIC))
      {
        var newBic = _obj.BIC.Trim();
        if (!_obj.BIC.Equals(newBic, StringComparison.InvariantCultureIgnoreCase))
          _obj.BIC = newBic;
      }
      
      if (!string.IsNullOrEmpty(_obj.CorrespondentAccount))
      {
        var newCorr = _obj.CorrespondentAccount.Trim();
        if (!_obj.CorrespondentAccount.Equals(newCorr, StringComparison.InvariantCultureIgnoreCase))
          _obj.CorrespondentAccount = newCorr;
      }
      
      // Проверить корректность SWIFT.
      var checkSwiftErrorText = Sungero.Parties.PublicFunctions.Bank.CheckSwift(_obj.SWIFT);
      if (!string.IsNullOrEmpty(checkSwiftErrorText))
        e.AddError(_obj.Info.Properties.SWIFT, checkSwiftErrorText);
      
      if (_obj.Nonresident != true)
      {
        if (_obj.BIC.Length != 5 && _obj.BIC.Length != 9)
          e.AddError(_obj.Info.Properties.BIC, "Поле МФО должен состоять из 5 или 9 цифр");
        
        // Проверить корректность корр. счета.
        var checkCorrErrorText = Sungero.Parties.PublicFunctions.Bank.CheckCorrLength(_obj.CorrespondentAccount);
        if (!string.IsNullOrEmpty(checkCorrErrorText))
          e.AddError(_obj.Info.Properties.CorrespondentAccount, checkCorrErrorText);
      }
      else
      {
        // Проверить корректность корр. счета для нерезидента.
        var checkCorrErrorText = Sungero.Parties.PublicFunctions.Bank.CheckCorrAccountForNonresident(_obj.CorrespondentAccount);
        if (!string.IsNullOrEmpty(checkCorrErrorText))
          e.AddError(_obj.Info.Properties.CorrespondentAccount, checkCorrErrorText);
      }
    }
  }

}