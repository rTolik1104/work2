using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using micros.DrxUzbekistan.City;

namespace micros.DrxUzbekistan.Server
{
  partial class CityFunctions
  {
    /// <summary>
    /// -- Create contract for --
    /// </summary>
    /// <returns></returns>
    [Remote, Public]
    public static ICity CreateCity()
    {
      return Cities.Create();
    }
    
    /// <summary>
    /// Получить все города
    /// </summary>
    [Remote, Public]
    public static List<ICity> GetAllCities()
    {
      if (!micros.DrxUzbekistan.Cities.Get().Equals(null))
        return micros.DrxUzbekistan.Cities.GetAll().ToList();
      else
        return null;
    }
  }
}