using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using Sungero.Core;
using Sungero.CoreEntities;

namespace micros.EimzoModule.Client
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

  }
}