using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using micros.MicrosSetting.Measurement;

namespace micros.MicrosSetting.Server
{
  partial class MeasurementFunctions
  {
    /// <summary>
    /// -- Get all measurement --
    /// </summary>
    /// <returns>-- List of companies --</returns>
    [Remote, Public]
    public static List<IMeasurement> GetAllMeasurement()
    {
      if(!micros.MicrosSetting.Measurements.GetAll().Equals(null))
      {
        return micros.MicrosSetting.Measurements.GetAll().ToList();
      }
      else
      {
        return null;
      }
    }
    
    [Remote, Public]
    public static IMeasurement CreateMeasurement()
    {
      return Measurements.Create();
    }
  }
}