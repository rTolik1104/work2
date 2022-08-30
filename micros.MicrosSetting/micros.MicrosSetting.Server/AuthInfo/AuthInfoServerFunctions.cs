using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using micros.MicrosSetting.AuthInfo;

namespace micros.MicrosSetting.Server
{
  partial class AuthInfoFunctions
  {
    /// <summary>
    /// Получить все доступы к API Multibank
    /// </summary>
    [Remote]
    public static List<micros.MicrosSetting.IAuthInfo> GetAllAccesses()
    {
      if (!micros.MicrosSetting.AuthInfos.Get().Equals(null))
        return micros.MicrosSetting.AuthInfos.GetAll().ToList();
      else
        return null;
    }
  }
}