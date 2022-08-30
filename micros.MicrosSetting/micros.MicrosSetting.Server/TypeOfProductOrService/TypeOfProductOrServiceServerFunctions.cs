using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using micros.MicrosSetting.TypeOfProductOrService;

namespace micros.MicrosSetting.Server
{
  partial class TypeOfProductOrServiceFunctions
  {
/// <summary>
		/// -- Create type of product or service --
		/// </summary>
		/// <returns></returns>
		[Remote, Public]
		public static ITypeOfProductOrService CreateTypeOfProductOrService()
		{
			return TypeOfProductOrServices.Create();
		}
		
		/// <summary>
		/// -- Get all product or service
		/// </summary>
		/// <returns></returns>
		[Remote, Public]
		public static List<ITypeOfProductOrService> GetAllTypeOfProductsOrServices()
		{
			if (!micros.MicrosSetting.TypeOfProductOrServices.Get().Equals(null))
				return micros.MicrosSetting.TypeOfProductOrServices.GetAll().ToList();
			else
				return null;
		}
  }
}