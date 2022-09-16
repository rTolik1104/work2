using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Company.Employee;
using Sungero.Docflow.Server;

namespace micros.EimzoModule.Server
{
  public class ModuleFunctions
  {
    /// <summary>
    /// Находит и возвращает все сертификаты в системе.
    /// </summary>
    /// <returns>Сертификаты пользователя</returns>
    [Remote]
    public IQueryable<ICertificate> GetAllCertificates()
    {
      return Certificates.GetAll();
    }
    
    [Remote]
    public static ICertificate CreateCertificate()
    {
      return Certificates.Create();
    }
  }
}