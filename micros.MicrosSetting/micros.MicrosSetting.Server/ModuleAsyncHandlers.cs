using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirectoryCity;
using micros.DrxUzbekistan.City;

namespace micros.MicrosSetting.Server
{
  public class ModuleAsyncHandlers
  {

    public virtual void CheckCity(micros.MicrosSetting.Server.AsyncHandlerInvokeArgs.CheckCityInvokeArgs args )
    {
      var cityDataList = DirectoryCity.City.GetCityDataList();
      var cityExist = micros.DrxUzbekistan.PublicFunctions.City.Remote.GetAllCities();
      
      foreach(var city in cityDataList)
      {
        var region = micros.DrxUzbekistan.PublicFunctions.Region.Remote.GetAllRegions().Where(r => r.Indexmicros == city.RegionIndex).FirstOrDefault();
        var country = micros.DrxUzbekistan.PublicFunctions.Country.Remote.GetAllCountries().Where(c => c.Indexmicros == city.CountryIndex).FirstOrDefault();
        if(cityExist.Where(c => c.Name == city.Name).Count() == 0 && country != null && region != null)
        {
          var newCity = micros.DrxUzbekistan.PublicFunctions.City.Remote.CreateCity();
          newCity.Name = city.Name;
          newCity.Status = micros.DrxUzbekistan.City.Status.Active;
          newCity.Region = region;
          newCity.Country = country;
          newCity.Save();
        }
      }
    }

  }
}