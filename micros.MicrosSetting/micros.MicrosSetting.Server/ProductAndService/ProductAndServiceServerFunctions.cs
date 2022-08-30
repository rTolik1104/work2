using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using micros.MicrosSetting.ProductAndService;

namespace micros.MicrosSetting.Server
{
  partial class ProductAndServiceFunctions
  {
    /// <summary>
    /// -- Create product or service --
    /// </summary>
    /// <returns></returns>
    [Remote, Public]
    public static IProductAndService CreateProductOrService()
    {
      return ProductAndServices.Create();
    }
    
    /// <summary>
    /// -- Get all product or service
    /// </summary>
    /// <returns></returns>
    [Remote, Public]
    public static List<IProductAndService> GetAllProductsOrServices()
    {
      if (!micros.MicrosSetting.ProductAndServices.Get().Equals(null))
        return micros.MicrosSetting.ProductAndServices.GetAll().ToList();
      else
        return null;
    }
  }
}