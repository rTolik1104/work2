using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using micros.DrxUzbekistan.Region;

namespace micros.DrxUzbekistan.Server
{
  partial class RegionFunctions
  {
    /// <summary>
    /// -- Create contract for --
    /// </summary>
    /// <returns></returns>
    [Remote, Public]
    public static IRegion CreateRegion()
    {
      return Regions.Create();
    }
    
    /// <summary>
    /// Получить все города
    /// </summary>
    [Remote, Public]
    public static List<IRegion> GetAllRegions()
    {
      if (!micros.DrxUzbekistan.Regions.Get().Equals(null))
        return micros.DrxUzbekistan.Regions.GetAll().ToList();
      else
        return null;
    }
  }
}