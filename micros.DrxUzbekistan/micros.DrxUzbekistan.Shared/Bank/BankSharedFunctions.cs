using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using micros.DrxUzbekistan.Bank;

namespace micros.DrxUzbekistan.Shared
{
  partial class BankFunctions
  {
    public override string CheckTin(string tin)
    {
      if (string.IsNullOrWhiteSpace(tin))
        return string.Empty;
      
      tin = tin.Trim();
      
      // Проверить содержание ИНН. Должен состоять только из цифр. (Bug 87755)
      if (!Regex.IsMatch(tin, @"^\d*$"))
        return Sungero.Parties.Counterparties.Resources.NotOnlyDigitsTin;
      
      // Проверить длину ИНН. Для компаний допустимы ИНН длиной 10 или 12 символов, для персон - только 12.
      if(tin.Length < 9 || tin.Length > 12)
      {
        return "ИНН должен состоять от 9 до 12 цифр.";
      }
      
      return string.Empty;
    }
    
    public void NoresidentFalse()
    {
      _obj.State.Properties.Code.IsVisible = false;
      _obj.State.Properties.TRRC.IsVisible = false;
      _obj.State.Properties.CorrespondentAccount.IsVisible = false;
    }
    
    public void NoresidentTrue()
    {
      _obj.State.Properties.Code.IsVisible = true;
      _obj.State.Properties.TRRC.IsVisible = true;
      _obj.State.Properties.CorrespondentAccount.IsVisible = true;
    }
  }
}