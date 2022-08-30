using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using micros.DrxUzbekistan.Country;

namespace micros.DrxUzbekistan.Server
{
  partial class CountryFunctions
  {
    /// <summary>
    /// -- Create contract for --
    /// </summary>
    /// <returns></returns>
    [Remote, Public]
    public static ICountry CreateCountry()
    {
      return Countries.Create();
    }
    
    /// <summary>
    /// Получить все города
    /// </summary>
    [Remote, Public]
    public static List<ICountry> GetAllCountries()
    {
      if (!micros.DrxUzbekistan.Countries.Get().Equals(null))
        return micros.DrxUzbekistan.Countries.GetAll().ToList();
      else
        return null;
    }
  }
}