using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Domain.Initialization;

namespace micros.MultibankSolution.Module.ExchangeCore.Server
{
  public partial class ModuleInitializer
  {

    public override void Initializing(Sungero.Domain.ModuleInitializingEventArgs e)
    {
      base.Initializing(e);
      CreateExchangeServiceMicros();
    }
    public static void CreateExchangeServiceMicros()
    {
      CreateExchangeServiceMultibank("https://app.multibank.uz/", micros.MultibankSolution.ExchangeService.ExchangeProvider.Multibank,
                                     "https://multibank.uz/oauth", false);
      
      CreateExchangeServiceMultibank("https://app-staging.multibank.uz/",micros.MultibankSolution.ExchangeService.ExchangeProvider.MultibankSTGmicros,
                                     "https://oauth-staging.multibank.uz/oauth", true);
    }
    
    /// <summary>
    /// Создать сервис обмена Multibank.
    /// </summary>
    /// <param name="uri">Ссылка на сервис.</param>
    /// <param name="exchangeProvider">Оператор обмена. ExchangeCore.ExchangeService.ExchangeProvider.</param>
    /// <param name="logonUrl">URL личного кабинета.</param>
    public static void CreateExchangeServiceMultibank(string uri, Enumeration exchangeProvider, string logonUrl, bool staging)
    {
      if (ExchangeServices.GetAll(s => Equals(s.ExchangeProvider, exchangeProvider)).Any())
        return;
      
      var system = ExchangeServices.Create();
      system.Name = ExchangeServices.Info.Properties.ExchangeProvider.GetLocalizedValue(exchangeProvider);
      system.Uri = uri;
      system.ExchangeProvider = exchangeProvider;
      system.LogonUrl = logonUrl;
      var r = micros.MultibankSolution.ExchangeServices.Resources;
      if (staging)
      {
        system.GetPDFmicros = r.GetPDFStaging;
        system.GetProfilemicros = r.GetProfileStaging;
        system.GetTokenmicros = r.GetTokenStaging;
        system.GetUpdatedmicros = r.GetUpdatedTokenStaging;
        system.GNKStringmicros = r.GNKStringStaging;
        system.JoinProfilemicros = r.JoinProfileStaging;
        system.MultibankAcceptmicros = r.MultibankAcceptStaging;
        system.RefreshTokenmicros = r.RefreshTokenStaging;
        system.SendInvoiceV2micros = r.SendInvoiceV2Staging;
        system.GetPublicDocmicros = r.GetPublicDocStaging;
        system.InvoiceURLmicros = r.InvoiceURLStaging;
        system.GetSignaturesmicros = r.GetSignaturesStaging;
        system.Cancelmicros = r.CancelStaging;
        system.Rejectmicros = r.RejectStaging;
        
        system.Save();
      }
      else
      {
        system.GetPDFmicros = r.GetPDF;
        system.GetProfilemicros = r.GetProfile;
        system.GetTokenmicros = r.GetToken;
        system.GetUpdatedmicros = r.GetUpdatedToken;
        system.GNKStringmicros = r.GNKString;
        system.JoinProfilemicros = r.JoinProfile;
        system.MultibankAcceptmicros = r.MultibankAccept;
        system.RefreshTokenmicros = r.RefreshToken;
        system.SendInvoiceV2micros = r.SendInvoiceV2;
        system.GetPublicDocmicros = r.GetPublicDoc;
        system.InvoiceURLmicros = r.InvoiceURL;
        system.GetSignaturesmicros = r.GetSignatures;
        system.Cancelmicros = r.Cancel;
        system.Rejectmicros = r.Reject;
        system.Save();
      }

      system.Save();
      Logger.Debug("GETPdf = " + system.GetPDFmicros);
    }
  }
}
