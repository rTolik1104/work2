using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using micros.DrxUzbekistan.BusinessUnit;

namespace micros.DrxUzbekistan.Server
{
  partial class BusinessUnitFunctions
  {
    /// <summary>
    /// Получить все банки
    /// </summary>
    [Remote]
    public static List<IBank> GetAllBanks()
    {
      if (!micros.DrxUzbekistan.Banks.Get().Equals(null))
        return micros.DrxUzbekistan.Banks.GetAll().ToList();
      else
        return null;
    }
  }
}