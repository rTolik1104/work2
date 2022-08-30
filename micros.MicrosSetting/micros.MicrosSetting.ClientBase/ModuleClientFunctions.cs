using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Sungero.Core;
using System.Security.Cryptography.X509Certificates;
using Sungero.CoreEntities;
using DirectoryCity;
using DirectoryRegion;
using DirectoryCountry;
using DirectoryBanks;
using DirectoryCounterpart;
using DirectoryMeasurement;
using micros.MicrosSetting;

namespace micros.MicrosSetting.Client
{
  public class ModuleFunctions
  {
    public virtual void ShowAllCertificates()
    {
      var listAllCertificates = Functions.Module.Remote.GetAllCertificates();
      listAllCertificates.Show();
    }

    /// <summary>
    /// Метод для извлечения открытого сертификата в формате .crt из контейнера PKCS#12 (файл .pfx).
    /// </summary>
    public virtual void CreateCertificate()
    {
      var dialog = Dialogs.CreateInputDialog("Выбор ключа");
      var certificate = dialog.AddFileSelect("Укажите необходимый ключ '*.pfx'", true);
      if (dialog.Show() == DialogButtons.Ok)
      {
        if (Path.GetExtension(certificate.Value.Name) == ".pfx")
        {
          var passDialog = Dialogs.CreateInputDialog("Введите пароль");
          var password = passDialog.AddPasswordString("Пароль", true);
          passDialog.Show();
          
          if (password.Value != null)
          {
            try
            {
              X509Certificate2 x509 = new X509Certificate2(certificate.Value.Content, password.Value);
              var certificateData = x509.Export(X509ContentType.Cert);
              //ImportCert(certificateData);
              
              var userCertificate = Functions.Module.Remote.CreateCertificate();
              //userCertificate.Owner = Users.Current;
              userCertificate.Description = "Для подписания";
              userCertificate.Enabled = true;
              userCertificate.X509Certificate = certificateData;
              userCertificate.Issuer = x509.GetNameInfo(X509NameType.SimpleName, true);
              
              // Срок действия сертификата из файла (NotAfter и NotBefore) возвращается в локальном времени машины, на которой исполняется данный код.
              userCertificate.NotAfter = x509.NotAfter.ToUniversalTime().FromUtcTime();
              userCertificate.NotBefore = x509.NotBefore.ToUniversalTime().FromUtcTime();
              userCertificate.Subject = x509.GetNameInfo(X509NameType.SimpleName, false);
              userCertificate.Thumbprint = x509.Thumbprint;
              userCertificate.Show();
            }
            catch (Exception ex)
            {
              Dialogs.ShowMessage(ex.Message, MessageType.Error);
            }
          }
        }
        else
        {
          Dialogs.ShowMessage("Требуемый объект не найден.", MessageType.Error);
        }
      }
    }

    /// <summary>
    /// 
    /// </summary>
    public virtual void AddProductAndService()
    {
      var adding = Functions.Module.Remote.AddProductAndService();
      if(adding == true)
      {
        Dialogs.ShowMessage("Продукции и услуги добавлены.", "", MessageType.Information, "Загрузка завершена");
      }
      else
      {
        Dialogs.ShowMessage("Что-то пошло не так.", "", MessageType.Question, "Ошибка");
      }
    }

    /// <summary>
    /// -- Add measurement --
    /// </summary>
    public virtual void AddMeasurements()
    {
      Dialogs.NotifyMessage("Пожалуйста подождите. Идет загрузка. Ожидайте завершения процесса.");
      var measurementDataList = DirectoryMeasurement.Measurement.GetMeasurementDataList();
      var measurementExist = MicrosSetting.PublicFunctions.Measurement.Remote.GetAllMeasurement();
      
      foreach(var measurement in measurementDataList)
      {
        if(measurementExist.Where(m => m.MeasureId == measurement.MeasurementIndex).Count() == 0)
        {
          var newMeasurement = MicrosSetting.PublicFunctions.Measurement.Remote.CreateMeasurement();
          newMeasurement.Name = measurement.Name;
          newMeasurement.MeasureId = measurement.MeasurementIndex;
          newMeasurement.Status = micros.DrxUzbekistan.Country.Status.Active;
          newMeasurement.Save();
        }
      }
      Dialogs.ShowMessage("Единицы измерения добавлены.", "", MessageType.Information, "Загрузка завершена");
    }

    /// <summary>
    /// -- Add banks of Uzbekistan --
    /// </summary>
    public virtual void AddUzBanks()
    {
      Dialogs.NotifyMessage("Пожалуйста подождите. Идет загрузка. Ожидайте завершения процесса.");
      var bankDataList = DirectoryBanks.Bank.GetBankDataList();
      
      foreach(var bank in bankDataList)
      {
        if(Sungero.Parties.Banks.GetAll().Where(b => b.BIC == bank.MFO).Count() == 0)
        {
          var newBank = Sungero.Parties.Banks.Create();
          newBank.BIC = bank.MFO;
          newBank.Name = bank.Name;
          newBank.Status = micros.DrxUzbekistan.Country.Status.Active;
          newBank.Save();
        }
      }
      Dialogs.ShowMessage("Банки добавлены.", "", MessageType.Information, "Загрузка завершена");
    }

    /// <summary>
    /// -- Add settlement of Uzbekistan --
    /// </summary>
    public virtual void AddUzSettlements()
    {
      Dialogs.NotifyMessage("Пожалуйста подождите. Идет загрузка населенных пунктов. Ожидайте завершения процесса.");
      AddUzbekistans();
      AddUzRegions();
      AddUzCities();
      Dialogs.ShowMessage("Населенные пункты добавлены.", "", MessageType.Information, "Загрузка завершена");
    }
    
    public virtual void AddUzbekistans()
    {
      var country = DirectoryCountry.Country.GetCountryDataList();
      var countyExist = micros.DrxUzbekistan.PublicFunctions.Country.Remote.GetAllCountries();
      
      foreach(var uzbekistan in country)
      {
        if(countyExist.Where(c => c.Name == uzbekistan.Name).Count() == 0 && countyExist.Where(c => c.Indexmicros == uzbekistan.CountryIndex).Count() == 0)
        {
          var newCountry = micros.DrxUzbekistan.PublicFunctions.Country.Remote.CreateCountry();
          newCountry.Name = uzbekistan.Name;
          newCountry.Code = uzbekistan.CountryCode.ToString();
          newCountry.Indexmicros = uzbekistan.CountryIndex;
          newCountry.Status = micros.DrxUzbekistan.Country.Status.Active;
          newCountry.Save();
        }
      }
    }
    
    public virtual void AddUzRegions()
    {
      var regionDataList = DirectoryRegion.Region.GetRegionDataList();
      var regionExist = micros.DrxUzbekistan.PublicFunctions.Region.Remote.GetAllRegions();
      
      foreach(var region in regionDataList)
      {
        var country = micros.DrxUzbekistan.PublicFunctions.Country.Remote.GetAllCountries().Where(c => c.Indexmicros == region.CountryIndex).FirstOrDefault();
        if(regionExist.Where(r => r.Name == region.Name).Count() == 0 && country != null && regionExist.Where(r => r.Indexmicros == region.RegionIndex).Count() == 0)
        {
          var newRegion = micros.DrxUzbekistan.PublicFunctions.Region.Remote.CreateRegion();
          newRegion.Name = region.Name;
          newRegion.Status = micros.DrxUzbekistan.City.Status.Active;
          newRegion.Country = country;
          newRegion.Indexmicros = region.RegionIndex;
          newRegion.Save();
        }
      }
    }
    
    public virtual void AddUzCities()
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