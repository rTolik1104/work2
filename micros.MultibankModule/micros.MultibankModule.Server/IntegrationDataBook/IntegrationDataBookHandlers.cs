using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using micros.MultibankModule.IntegrationDataBook;

namespace micros.MultibankModule
{
  partial class IntegrationDataBookServerHandlers
  {

    public override void Created(Sungero.Domain.CreatedEventArgs e)
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
      _obj.GetSignatures = r.GetSignatures;
      if (Sungero.Company.Employees.Current.Department != null)
        _obj.Account = MultibankModule.AuthDatabooks.GetAll().Where(x => x.BusinessUnit == Sungero.Company.Employees.Current.Department.BusinessUnit).FirstOrDefault();
      //_obj.Save();
    }
  }

}