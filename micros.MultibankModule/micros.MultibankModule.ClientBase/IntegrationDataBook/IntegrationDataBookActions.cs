using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using micros.MultibankModule.IntegrationDataBook;

namespace micros.MultibankModule.Client
{
  partial class IntegrationDataBookActions
  {
    public virtual void LoadStaging(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      var r = MultibankModule.IntegrationDataBooks.Resources;
      _obj.GetPDF = r.GetPDFStaging;
      _obj.GetProfile = r.GetProfileStaging;
      _obj.GetToken = r.GetTokenStaging;
      _obj.GetUpdatedToken = r.GetUpdatedTokenStaging;
      _obj.GNKString = r.GNKStringStaging;
      _obj.JoinProfile = r.JoinProfileStaging;
      _obj.MultibankAccept = r.MultibankAcceptStaging;
      _obj.RefreshToken = r.RefreshTokenStaging;
      _obj.SendInvoiceV2 = r.SendInvoiceV2Staging;
      _obj.GetPublicDoc = r.GetPublicDocStaging;
      _obj.InvoiceURL = r.InvoiceURLStaging;
      _obj.GetSignatures = r.GetSignaturesStaging;
      _obj.Save();
    }

    public virtual bool CanLoadStaging(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return true;
    }

    public virtual void LoadProdaction(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      var r = MultibankModule.IntegrationDataBooks.Resources;
      _obj.GetPDF = r.GetPDF;
      _obj.GetProfile = r.GetProfile;
      _obj.GetToken = r.GetToken;
      _obj.GetUpdatedToken = r.GetUpdatedToken;
      _obj.GNKString = r.GNKString;
      _obj.JoinProfile = r.JoinProfile;
      _obj.MultibankAccept = r.MultibankAccept;
      _obj.RefreshToken = r.RefreshToken;
      _obj.SendInvoiceV2 = r.SendInvoiceV2;
      _obj.GetPublicDoc = r.GetPublicDoc;
      _obj.InvoiceURL = r.InvoiceURL;
      _obj.GetSignatures = r.GetSignatures;
      _obj.Save();
    }

    public virtual bool CanLoadProdaction(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return true;
    }

  }

}