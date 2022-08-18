using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using RestSharp.Authenticators;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Cms;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Cms;


namespace micros.MultibankModule.Server
{
  public class ModuleFunctions
  {
    #region Base64, Формирование Json и тд
    
    /// <summary>
    /// Конвертация(Кодировка) Json в Base64.
    /// </summary>
    /// <param name="obj">Json типа object</param>
    /// <returns>Json закодированный в Base64 String</returns>
    private string Base64EncodeObject(object obj)
    {
      var plainTextBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(obj));
      return Convert.ToBase64String(plainTextBytes);
    }
    
    /// <summary>
    /// Конвертация(Декодировка) Base64 в Json.
    /// </summary>
    /// <param name="base64String">json закодированный в Base64</param>
    /// <returns>Json</returns>
    //    private Newtonsoft.Json.Linq.JObject Base64DecodeObject(string base64String)
    //    {
    //      var base64EncodedBytes = Convert.FromBase64String(base64String);
    //      string jsonString = Encoding.UTF8.GetString(base64EncodedBytes);
    //      //JArray jArray = JArray.Parse(jsonString);
    //      //JObject json = JsonConvert.DeserializeObject<JObject>(jsonString);
    //      JObject json = JObject.Parse(jsonString);
    //      return json; //JsonConvert.DeserializeObject<JObject>(Encoding.UTF8.GetString(base64EncodedBytes));
    //    }
    private string Base64DecodeObject(string base64String)
    {
      var base64EncodedBytes = Convert.FromBase64String(base64String);
      string json = Encoding.UTF8.GetString(base64EncodedBytes);
      //JArray jArray = JArray.Parse(jsonString);
      //JObject json = JsonConvert.DeserializeObject<JObject>(jsonString);
      //JObject json = JObject.Parse(jsonString);
      return json; //JsonConvert.DeserializeObject<JObject>(Encoding.UTF8.GetString(base64EncodedBytes));
    }
    
    /// <summary>
    /// Создание Json ЭСФ 2.0 (Черновик) и заполнение.
    /// </summary>
    /// <param name="version">Версия документа</param>
    /// <returns>Json ЭСФ 2.0 (Черновик)</returns>
    [Public]
    public string FillInvoiceV2(Sungero.Content.IElectronicDocumentVersions version)
    {
      JObject json;
      JObject invoiceV2;
      
      try
      {
        using (var sr = new StreamReader(version.Body.Read()))
          using (var jsonTextReader = new JsonTextReader(sr))
            json = JObject.Parse(new JsonSerializer().Deserialize(jsonTextReader).ToString());
        
        invoiceV2 = new JObject(
          new JProperty("agree_to_fine", json.SelectToken("document.agree_to_fine")),
          new JProperty("old_invoice_doc",
                        new JObject(
                          new JProperty("old_invoice_id", json.SelectToken("document.old_invoice_doc.old_invoice_id")),
                          new JProperty("old_invoice_number", json.SelectToken("document.old_invoice_doc.old_invoice_number")),
                          new JProperty("old_invoice_date", json.SelectToken("document.old_invoice_doc.old_invoice_date")))),
          new JProperty("contract",
                        new JObject(
                          new JProperty("document_contract_date", json.SelectToken("document.contract.document_contract_date")),
                          new JProperty("document_contract_no", json.SelectToken("document.contract.document_contract_no")))),
          new JProperty("is_one_sided_invoice", json.SelectToken("document.is_one_sided_invoice")),
          new JProperty("foreign_counterparty",
                        new JObject(
                          new JProperty("country_id", json.SelectToken("document.foreign_counterparty.foreign_country_id")),
                          new JProperty("foreign_counterparty_name", json.SelectToken("document.foreign_counterparty.foreign_counterparty_name")),
                          new JProperty("foreign_counterparty_address", json.SelectToken("document.foreign_counterparty.foreign_counterparty_address")),
                          new JProperty("foreign_counterparty_bank", json.SelectToken("document.foreign_counterparty.foreign_counterparty_bank")),
                          new JProperty("foreign_counterparty_account", json.SelectToken("document.foreign_counterparty.foreign_counterparty_account")))),
          new JProperty("power_of_attorney",
                        new JObject(
                          new JProperty("attorney_no", json.SelectToken("document.power_of_attorney.attorney_no")),
                          new JProperty("attorney_date_of_issue", json.SelectToken("document.power_of_attorney.attorney_date_of_issue")),
                          new JProperty("attorney_confidant_tin", json.SelectToken("document.power_of_attorney.attorney_confidant_tin")),
                          new JProperty("attorney_confidant_person_id", json.SelectToken("document.power_of_attorney.attorney_confidant_person_id")),
                          new JProperty("attorney_confidant_fio", json.SelectToken("document.power_of_attorney.attorney_confidant_fio")))),
          new JProperty("storekeeper",
                        new JObject(
                          new JProperty("storekeeper_tin", json.SelectToken("document.storekeeper.storekeeper_tin")),
                          new JProperty("storekeeper_full_name", json.SelectToken("document.storekeeper.owner_released_the_goods_name")),
                          new JProperty("storekeeper_person_id", json.SelectToken("document.storekeeper.storekeeper_person_id")))),
          new JProperty("contragent_tin", json.SelectToken("document.contragent_tin")),
          new JProperty("has_medical", json.SelectToken("document.has_medical")),
          new JProperty("with_excise", json.SelectToken("document.with_excise")),
          new JProperty("has_committent", json.SelectToken("document.has_committent")),
          new JProperty("invoice",
                        new JObject(
                          new JProperty("document_date", json.SelectToken("document.invoice.document_date")),
                          new JProperty("document_number", json.SelectToken("document.invoice.document_number")))),
          new JProperty("one_sided_invoice_type", json.SelectToken("document.one_sided_invoice_type").ToString()),
          new JProperty("invoice_type", json.SelectToken("document.invoice_type")),
          new JProperty("has_exemption", json.SelectToken("document.invoice_type")),
          new JProperty("hide_report_commitent", json.SelectToken("document.hide_report_commitent")),
          new JProperty("contragent_account", json.SelectToken("document.contragent.contragent_account")),
          new JProperty("contragent_address", json.SelectToken("document.contragent.contragent_address")),
          new JProperty("owner_address", json.SelectToken("document.owner.owner_address")),
          new JProperty("owner_account", json.SelectToken("document.owner.owner_account")),
          new JProperty("owner_bank_id", json.SelectToken("document.owner.owner_bank_id")),
          new JProperty("contragent_bank_id", json.SelectToken("document.contragent.contragent_bank_id")),
          new JProperty("document_number", json.SelectToken("document.document_number")),
          new JProperty("document_date", json.SelectToken("document.document_date")),
          new JProperty("has_vat", json.SelectToken("document.has_vat")),
          new JProperty("goods", json.SelectToken("document.goods")),
          new JProperty("owner_vat_registration_code", json.SelectToken("document.owner.owner_vat_registration_code")),
          new JProperty("contragent_vat_registration_code", json.SelectToken("document.contragent.contragent_vat_registration_code")));
      }
      catch
      {
        return "null";  // Приходится возвращать текст null потому-что это не C# 8.0 -_-
      }
      
      //--------------------------------------------------------------------------------
      var doc = Sungero.Docflow.SimpleDocuments.Create();
      doc.Name = "test";
      doc.Subject = "test";
      doc.CreateVersion();
      var  ver = doc.LastVersion;
      byte[] jsonByte = Convert.FromBase64String(Base64EncodeObject(invoiceV2));
      MemoryStream jsonStream = new MemoryStream(jsonByte);
      ver.Body.Write(jsonStream);
      ver.BodyAssociatedApplication = Sungero.Content.AssociatedApplications.GetByExtension("json");
      ver.AssociatedApplication = Sungero.Content.AssociatedApplications.GetByExtension("json");
      doc.Save();
      //--------------------------------------------------------------------------------
      
      return invoiceV2.ToString();
    }
    
    #endregion
    
    #region Запросы к мультибанку
    
    /// <summary>
    /// Получения PDF документа в формате base64 из Мультибанка.
    /// (1. Запрос на создание PDF по document_id. 2. Как ответ получить PDF в формате base64)
    /// </summary>
    /// <param name="json">Json для получение ID документа и Пароль</param>
    /// <returns>PDF в Base64, если PDF документ был не получен, возвращает текст - "False"</returns>
    public string GetPDF(Newtonsoft.Json.Linq.JObject json)
    {
      var box = this.GetIntegrationDataBookForCurrentUser();
        var client = new RestClient(box.ExchangeService.GetPDFmicros);
      var request = new RestRequest(Method.POST);
      request.AddParameter("is_new", "false");
      request.AddParameter("document_id", json.SelectToken("document.document_id").ToString());
      request.AddParameter("password", json.SelectToken("document.document_password_owner").ToString());
      IRestResponse response = client.Execute(request);
      return response.IsSuccessful ? JObject.Parse(response.Content).SelectToken("data").ToString() : JObject.Parse(response.Content).SelectToken("success").ToString();
    }
    
    /// <summary> Создание ЭСФ версии 2.0 в Мультибанке. </summary>
    /// <param name="jsonString">Json для отправки(ЭСФ 2.0).</param>
    /// <param name="token">Токен доступа</param>
    /// <returns>Ответ в виде string</returns>
    [Public]
    public string SendDocument(string jsonString, string token)
    {
      JObject json = JObject.Parse(jsonString);
      var client = new RestClient(MultibankSolution.BusinessUnitBoxes.GetAll().Where(x => x.BusinessUnit == Sungero.Company.Employees.Current.Department.BusinessUnit)
                                  .Where(b => b.ExchangeService.ExchangeProvider == micros.MultibankSolution.ExchangeService.ExchangeProvider.Multibank ||
                                         b.ExchangeService.ExchangeProvider == micros.MultibankSolution.ExchangeService.ExchangeProvider.MultibankSTGmicros).FirstOrDefault().ExchangeService.SendInvoiceV2micros);
      client.Authenticator = new JwtAuthenticator(token);
      var request = new RestRequest(Method.POST);
      request.AddParameter("doctype_id", Constants.Module.DoctypeId.Invoice);
      request.AddParameter("data", json);
      IRestResponse response = client.Execute(request);
      return response.Content;
    }
    
    /// <summary> Получение токена доступа, обновлений и срок действие </summary>
    /// <param name="clientId">Идентификатор приложения запрашивающего токен</param>
    /// <param name="clientSecret">Ключ приложения запрашивающего токен</param>
    /// <param name="userName">Логин пользователя</param>
    /// <param name="password">Пароль пользователя</param>
    /// <returns>Токен доступа, обновлений и срок действие в виде Json</returns>
    [Public]
    public string AuthWithLogin(string clientId, string clientSecret, string userName, string password, bool staging)
    {
      string address = "";
      if (!staging)
        address = "https://account.multibank.uz/api/login";
      else
        address = "https://account-staging.multibank.uz/api/login";
      var client = new RestClient(address);
      client.Timeout = -1;
      var request = new RestRequest(Method.POST);
      request.AlwaysMultipartFormData = true;
      request.AddParameter("grant_type", "password"); //  Грант тип , авторизации
      request.AddParameter("client_id", clientId);
      request.AddParameter("client_secret", clientSecret);
      request.AddParameter("username", userName);
      request.AddParameter("password", password);
      request.AddParameter("scope", "*"); //  Массив прав разделенных пробелами выдаваемых токену.
      IRestResponse response = client.Execute(request);
      return response.Content;
    }
    
    /// <summary> Обновление токена. </summary>
    /// <param name="refreshToken">Токен обновление</param>
    /// <returns>Получение нового токена доступа, обновлений и срок действий в виде Json</returns>
    [Public]
    public string RefreshToken(string refreshToken)
    {
      var box = this.GetIntegrationDataBookForCurrentUser();
        var client = new RestClient(box.ExchangeService.GetUpdatedmicros);
      client.Timeout = -1;
      var request = new RestRequest(Method.POST);
      request.AlwaysMultipartFormData = true;
      request.AddParameter("refresh_token", refreshToken);
      request.AddParameter("client_id", box.ClientIdmicros);
      request.AddParameter("client_secret", box.ClientSecretmicros);
      request.AddParameter("scope", "*");
      IRestResponse response = client.Execute(request);
      return response.Content;
    }
    
    /// <summary> Получение информации о токене обновления </summary>
    /// <param name="refreshToken">Токен обновление</param>
    /// <returns>Вернуть True в случае если токен ещё действителен, иначе False</returns>
    [Public]
    public bool isToken(string refreshToken)
    {
      var client = new RestClient(MultibankModule.IntegrationDataBooks.GetAll().Where(x => x.Account == micros.MultibankModule.AuthDatabooks.GetAll().Where(a => a.BusinessUnit == Sungero.Company.Employees.Current.Department.BusinessUnit).FirstOrDefault()).FirstOrDefault().RefreshToken + refreshToken);
      var request = new RestRequest(Method.GET);
      IRestResponse response = client.Execute(request);
      return DateTimeOffset.Now.ToUnixTimeSeconds() + 10 < ((long)JObject.Parse(response.Content).SelectToken("expire_time")) ? true : false;
    }
    
    /// <summary>
    /// Метод получения списка доступных для авторизации профилей юридических и физических лиц
    /// </summary>
    /// <param name="token">Токен доступа</param>
    /// <returns></returns>
    [Public]
    public string GetProfile(string token)
    {
      var client = new RestClient(this.GetIntegrationDataBookForCurrentUser().ExchangeService.GetProfilemicros);
      client.Authenticator = new JwtAuthenticator(token);
      var request = new RestRequest(Method.GET);
      request.AlwaysMultipartFormData = true;
      IRestResponse response = client.Execute(request);
      Console.WriteLine(response.Content);
      return response.Content;
    }
    
    /// <summary>
    /// Метод необходим для присоединения профиля компании или же физического лица к токену.
    /// </summary>
    /// <param name="token">Токен доступа</param>
    /// <param name="profileId">Идентификатор профиля для присоеденения к токенуа</param>
    /// <returns></returns>
    [Public]
    public string JoinProfile(string token, string profileId)
    {
      var client = new RestClient(this.GetIntegrationDataBookForCurrentUser().ExchangeService.JoinProfilemicros);
      client.Authenticator = new JwtAuthenticator(token);
      var request = new RestRequest(Method.POST);
      request.AlwaysMultipartFormData = true;
      request.AddParameter("profile_id", profileId);
      IRestResponse response = client.Execute(request);
      Console.WriteLine(response.Content);
      return "";
    }
    
    #endregion
    
    #region API CRUD
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="parameter">Json закодированный в Base64</param>
    /// <returns>Метод отработан</returns>
    [Public(WebApiRequestType = RequestType.Post)]
    public virtual string CreateDocument(string parameter)
    {
      //JArray jArray = JArray.Parse(Base64DecodeObject(parameter).ToString());
      var json = JObject.Parse(JsonConvert.DeserializeObject<JObject>(Base64DecodeObject(parameter)).ToString());
      
      // Проверка ИНН
      if(json.SelectToken("document.document_data.SellerTin").ToString() == "" || json.SelectToken("document.document_data.BuyerTin").ToString() == "")
        return "Инн покупателя или продавца не заполнена";
      
      // Создание документа по типу
      string doctype = json["document"]["doctype_id"].ToString();
      if(doctype == Constants.Module.DoctypeId.Invoice)
        return Create_IncomingInvoice(json);
      else if(doctype == Constants.Module.DoctypeId.Actum)
        return "Неверный тип документа";
      else if(doctype == Constants.Module.DoctypeId.GnkContract)
        return "Неверный тип документа";
      else if(doctype == Constants.Module.DoctypeId.Contract)
        return "Неверный тип документа";
      else if(doctype == Constants.Module.DoctypeId.PowerOfAttorney)
        return "Неверный тип документа";
      else if(doctype == Constants.Module.DoctypeId.TTH)
        return "Неверный тип документа";
      
      return "Неверный тип документа";
    }
    
    #endregion
    
    #region Документы
    
    /// <summary> Создание документа "Счет-фактура полученный" - (IncomingTaxInvoice) </summary>
    /// <param name="json">Json по которому будет создан документ</param>
    /// <returns>Ответ об успехе</returns>
    public virtual string Create_IncomingInvoice(Newtonsoft.Json.Linq.JObject json)
    {
      var box = this.GetIntegrationDataBookForCurrentUser();
      this.IfNeedRefreshTokens(box);
      if(!micros.MultibankSolution.IncomingTaxInvoices.GetAll().Any(x => x.Guidmicros == json.SelectToken("document.document_id").ToString()))
      {
        var document = micros.MultibankSolution.IncomingTaxInvoices.Create();
        document.Save();
        this.UpdateInvoiceFromJson(json.ToString(), document);
        var task = micros.MultibankModule.ReviewTasks.Create();
        task.AttachmentGroup.OfficialDocuments.Add(document);
        task.Subject = "Получен документ : " + document.Name;
        task.Start();
        return "OK - " + document.Info.ToString();
      }
      else
      {
        var document = micros.MultibankSolution.IncomingTaxInvoices.GetAll().Where(x => x.Guidmicros == json.SelectToken("document.document_id").ToString()).FirstOrDefault();
        this.UpdateInvoiceFromJson(json.ToString(), document);
        return "OK - " + document.Info.ToString();
      }
    }
    
    /// <summary>
    /// Обновить вх СФ из json
    /// </summary>
    /// <param name="jsonString">новый json</param>
    /// <param name="documentOld">старый документ</param>
    [Public, Remote]
    public virtual void UpdateInvoiceFromJson(string jsonString, Sungero.Content.IElectronicDocument documentOld)
    {
      var document = micros.MultibankSolution.IncomingTaxInvoices.Get(documentOld.Id);
      JObject json = JObject.Parse(jsonString);
      string route = "document.";
      string buyerTin = json.SelectToken(route + "document_data.BuyerTin").ToString();
      string sellerTin = json.SelectToken(route + "document_data.SellerTin").ToString();
      
      document.Guidmicros = json.SelectToken("document.document_id").ToString();
      document.Currency = Sungero.Commons.Currencies.GetAll().Where(x => x.AlphaCode == "UZS").FirstOrDefault();
      if(json.SelectToken("document.goods.[0].total_sum") != null)
        document.TotalAmount = Convert.ToDouble(json.SelectToken("document.goods.[0].total_sum"));
      document.BusinessUnit = Sungero.Company.BusinessUnits.GetAll().Where(x => x.TIN == buyerTin).FirstOrDefault();
      document.Counterparty = Sungero.Parties.Companies.GetAll().Where(x => x.TIN == sellerTin).FirstOrDefault();
      document.RegistrationNumber = json.SelectToken("document.document_number").ToString();
      document.RegistrationDate = Convert.ToDateTime(json.SelectToken("document.document_date").ToString());
      //  Если свойства Наша организация или Контрагент пустая, создать новую на основе данных из json
      if(document.BusinessUnit == null) document.BusinessUnit = Create_BuisnessUnit(json.SelectToken(route + "document_data.Buyer.Name").ToString(), buyerTin, json.SelectToken(route + "document_data.Buyer.Address").ToString(), json.SelectToken(route + "document_data.Buyer.Account").ToString(), json.SelectToken(route + "document_data.Buyer.VatRegCode").ToString(), json.SelectToken(route + "document_data.Buyer.BankId").ToString());
      if(document.Counterparty == null) document.Counterparty = Create_Counterparty(json.SelectToken(route + "document_data.Seller.Name").ToString(), sellerTin, json.SelectToken(route + "document_data.Seller.Address").ToString(), json.SelectToken(route + "document_data.Seller.Account").ToString(), json.SelectToken(route + "document_data.Seller.VatRegCode").ToString(), json.SelectToken(route + "document_data.Seller.BankId").ToString());

      document.CreateVersion();
      var  version = document.LastVersion;
      
      
      
      byte[] jsonByte = Encoding.Default.GetBytes(jsonString);
      MemoryStream jsonStream = new MemoryStream(jsonByte);
      version.Body.Write(jsonStream);
      version.BodyAssociatedApplication = Sungero.Content.AssociatedApplications.GetByExtension("json");
      
      string pdfBase64 = GetPDF(JObject.Parse(json.ToString()));
      if(pdfBase64 != "false" && pdfBase64 != "False" && pdfBase64 != null)
      {
        byte[] pdfByte = Convert.FromBase64String(pdfBase64);
        MemoryStream pdfStream = new MemoryStream(pdfByte);
        version.PublicBody.Write(pdfStream);
        version.AssociatedApplication = Sungero.Content.AssociatedApplications.GetByExtension("pdf");
      }
      else
        version.AssociatedApplication = Sungero.Content.AssociatedApplications.GetByExtension("json");
      
      string status = json.SelectToken("transaction.transaction_operation").ToString();
      string statusNewValue = GetStatusFromMultibank(status);
      if(statusNewValue != null && statusNewValue != document.LifeCycleState.Value.Value)
      {
        document.LifeCycleState = document.LifeCycleStateAllowedItems.Where(n => n.Value.ToString() == statusNewValue).FirstOrDefault();
        document.UpdateStatusDatemicros = Calendar.Now;
      }
      
      document.JSonDocumentmicros = jsonByte;
      
      document.Save();
      
      this.RegistrSigning(JObject.Parse(json.SelectToken("signature_information").ToString()), document);
    }
    #endregion
    
    #region Создание записей в справочнике
    
    /// <summary> Создание сущности "Наша орг." </summary>
    /// <param name="name">Название</param>
    /// <param name="tin">ИНН</param>
    /// <param name="address">Адрес</param>
    /// <param name="account">Банковский счёт</param>
    /// <param name="vat">Регистрационный код плательщика НДС</param>
    /// <param name="bankMFO">Банк МФО</param>
    /// <returns>Созданный орг.</returns>
    private Sungero.Company.IBusinessUnit Create_BuisnessUnit(string name, string tin, string address, string account, string vat, string bankMFO)
    {
      var book = Sungero.Company.BusinessUnits.Create();
      book.Name = name;
      book.TIN = tin;
      book.LegalAddress = address;
      book.Save();
      return Sungero.Company.BusinessUnits.GetAll().Where(x => x.TIN == tin).FirstOrDefault();
    }
    
    /// <summary> Создание сущности "Организация" </summary>
    /// <param name="name">Название</param>
    /// <param name="tin">ИНН</param>
    /// <param name="address">Адрес</param>
    /// <param name="account">Банковский счёт</param>
    /// <param name="vat">Регистрационный код плательщика НДС</param>
    /// <param name="bankMFO">Банк МФО</param>
    /// <returns>Созданный орг.</returns>
    private Sungero.Parties.ICounterparty Create_Counterparty(string name, string tin, string address, string account, string vat, string bankMFO)
    {
      var book = Sungero.Parties.Companies.Create();
      book.Name = name;
      book.TIN = tin;
      book.LegalAddress = address;
      book.Save();
      return Sungero.Parties.Counterparties.GetAll().Where(x => x.TIN == tin).FirstOrDefault();
    }
    
    #endregion
    
    #region Sign-!DEV!
    
    /// <summary>
    /// Получить строку PKCS7 для подписания в тело документа
    /// </summary>
    /// <param name="document">Документ полученный из Мультибанка, который нужно подписать</param>
    [Public, Remote]
    public void GetMultibankSignData(Sungero.Content.IElectronicDocument document)
    {
      string document_id = MultibankSolution.IncomingTaxInvoices.Get(document.Id).Guidmicros.ToString();

      string gnkString = this.GNKString(document_id);
      byte[] jsonByte = Convert.FromBase64String(Base64EncodeObject(gnkString));
      MemoryStream jsonStream = new MemoryStream(jsonByte);
      var version = document.LastVersion;
      if (Locks.GetLockInfo(version.Body).IsLocked)
        version.Body.Write(jsonStream);
      
      document.Save();
    }
    
    /// <summary>
    /// Открепить подпись от документа
    /// </summary>
    /// <param name="signature"></param>
    /// <returns></returns>
    public byte[] ConverSignatureToDeattached(byte[] signature)
    {
      var asn1Object = Asn1Object.FromByteArray(signature);
      var contentInfo = ContentInfo.GetInstance(asn1Object);
      var signedData = SignedData.GetInstance(contentInfo.Content);
      var newEncapContentInfo = new ContentInfo(CmsObjectIdentifiers.Data, null);
      var newSignedData = new SignedData(signedData.DigestAlgorithms, newEncapContentInfo, signedData.Certificates, signedData.CRLs, signedData.SignerInfos);
      var newContentInfo = new ContentInfo(contentInfo.ContentType, newSignedData);
      return newContentInfo.GetDerEncoded();
    }

    /// <summary>
    /// Получить строку PKCS7 из Multibank
    /// </summary>
    /// <param name="document_id">GUID документа в Multibank</param>
    /// <returns></returns>
    [Remote]
    public string GNKString(string document_id)
    {
      //var invoice = MultibankSolution.IncomingTaxInvoices.Get(document.Id);
      var client = new RestClient(this.GetIntegrationDataBookForCurrentUser().ExchangeService.GNKStringmicros);
      client.Authenticator = new JwtAuthenticator(this.GetMultibankToken());
      var request = new RestRequest(Method.POST);
      request.AddParameter("document_id", document_id);
      request.AddParameter("action", "accept");
      IRestResponse response = client.Execute(request);
      JObject json = JObject.Parse(response.Content.ToString());
      string forSign = json.SelectToken("data.string").ToString();
      Logger.Debug("Test_pcs7: " + forSign);
      return forSign;
    }

    /// <summary>
    /// Возвращает подписанные данные в Multibank
    /// </summary>
    /// <param name="document"></param>
    [Public, Remote]
    public void ReturnSigningDocumentInMultibank(Sungero.Content.IElectronicDocument document)
    {
      var sugnatures = Signatures.Get(document.LastVersion).Where(x => x.SignCertificate != null);
      
      byte[] finalSignature = null;

      foreach(var sugnature in sugnatures.OrderBy(s => s.SigningDate))
      {
        if(finalSignature == null)
        {
          finalSignature = sugnature.GetDataSignature();
          continue;
        }
        
        finalSignature = this.MergeSignatures(finalSignature, sugnature.GetDataSignature());
      }

      using (var ms = new System.IO.MemoryStream())
      {
        document.LastVersion.Body.Read().CopyTo(ms);
        finalSignature = this.ConverSignatureToAttached(finalSignature, ms.ToArray());
      }
      
      var client = new RestClient(MultibankSolution.BusinessUnitBoxes.GetAll().Where(x => x.BusinessUnit == Sungero.Company.Employees.Current.Department.BusinessUnit)
                                  .Where(b => b.ExchangeService.ExchangeProvider == micros.MultibankSolution.ExchangeService.ExchangeProvider.Multibank ||
                                         b.ExchangeService.ExchangeProvider == micros.MultibankSolution.ExchangeService.ExchangeProvider.MultibankSTGmicros).FirstOrDefault().ExchangeService.MultibankAcceptmicros);
      string token = this.GetMultibankToken();
      string document_id = micros.MultibankSolution.IncomingTaxInvoices.Get(document.Id).Guidmicros;
      client.Authenticator = new JwtAuthenticator(token);
      var request = new RestRequest(Method.POST);
      request.AddParameter("document_id", document_id);
      request.AddParameter("sign", Convert.ToBase64String(finalSignature));
      Logger.Debug("pkcs7: " + Convert.ToBase64String(finalSignature));
      IRestResponse response = client.Execute(request);
      Logger.Debug(response.Content);
    }
    
    [Public, Remote]
    public micros.MultibankSolution.IBusinessUnitBox GetIntegrationDataBookForCurrentUser()
    {
      return MultibankSolution.BusinessUnitBoxes.GetAll().Where(x => x.BusinessUnit == Sungero.Company.Employees.Current.Department.BusinessUnit)
        .Where(b => b.ExchangeService.ExchangeProvider == micros.MultibankSolution.ExchangeService.ExchangeProvider.Multibank ||
               b.ExchangeService.ExchangeProvider == micros.MultibankSolution.ExchangeService.ExchangeProvider.MultibankSTGmicros).FirstOrDefault();
    }

    
    #endregion
    
    
    
    #region Здесь был Андрей
    
    #region Create PDF
    //    [Public, Remote]
    //    public void CreatePDF(Sungero.Content.IElectronicDocument document)
    //    {
    //      var converter = Sungero.AsposeExtensions.Converter.Create();
    //      string html = "";
    //      if (document.GetEntityMetadata().GetOriginal().NameGuid.ToString() == "74c9ddd4-4bc4-42b6-8bb0-c91d5e21fb8a")
    //        html = micros.Multibank.Resources.IncomingTaxInvoiceHTML;
    //      byte[] bytes = Encoding.UTF8.GetBytes(html);
    //      using (MemoryStream stringStream = new MemoryStream(bytes))
    //      {
    //        var htmlToPDF = converter.ConvertHtmlToPdf(stringStream);
//
    //        if (document.LastVersion == null)
    //        {
    //          document.Versions.AddNew();
    //          this.WriteVersion(document, htmlToPDF);
    //        }
    //        else
    //          this.WriteVersion(document, htmlToPDF);
    //      }
    //    }
    //    public void WriteVersion(Sungero.Content.IElectronicDocument document, System.IO.MemoryStream htmlToPDF)
    //    {
    //      var version = document.LastVersion;
    //      version.PublicBody.Write(htmlToPDF);
    //      version.AssociatedApplication = Sungero.Content.AssociatedApplications.GetByExtension("pdf");
//
    //      document.Save();
    //      //version.Save();
    //    }
    //    public virtual string CreateGoodsTable(Sungero.Content.IElectronicDocument document)
    //    {
    //      string d = document.Versions.FirstOrDefault().Body.ToString();
    //      var json = Newtonsoft.Json.Linq.JObject.Parse(d);
    //      var allGoods = json.SelectToken("data.document.goods").ToList();
    //      string allgoodstable = " ";
    //      foreach (var good in allGoods)
    //      {
    //        string goodstring = micros.Multibank.Resources.GoodTableHTML;
    //        goodstring.Replace("{Good name}",good.SelectToken("Name").ToString());
    //        goodstring.Replace("{IKPU}",good.SelectToken("Name").ToString());
    //        goodstring.Replace("{Units",good.SelectToken("unit").ToString());
    //        goodstring.Replace("{Quantity}",good.SelectToken("unit").ToString());
    //        goodstring.Replace("{Cost}",good.SelectToken("price").ToString());
    //        goodstring.Replace("{Price}",good.SelectToken("amount").ToString());
    //        goodstring.Replace("{Bid}",good.SelectToken("Name").ToString());
    //        goodstring.Replace("{Summ}",good.SelectToken("Summa").ToString());
    //        goodstring.Replace("{NDSCost}",good.SelectToken("DeliverySumWithVat").ToString());
    //        allgoodstable = allgoodstable + goodstring;
    //      }
    //      return allgoodstable;
    //    }
    #endregion
    
    #region Sign document in multibank
    /// <summary>
    /// Смотрит, подписан ли документ в Multibank
    /// </summary>
    /// <param name="docId">ID документа в Multibank</param>
    /// <param name="docPassword">Пароль документа в Multibank</param>
    /// <returns></returns>
    [Public, Remote]
    public virtual bool IsSignedInMultibank(Sungero.Content.IElectronicDocument document)
    {
      string json = this.GetMultibankJsonFromID(document);
      JObject jsonIn = JObject.Parse(json);
      if (jsonIn.SelectToken("data.signature_information.contragent") == null && jsonIn.SelectToken("data.signature_information.contragent") == null)
        return false;
      else
      {
        return true;
      }
    }
    
    /// <summary>
    /// Импорт дописи документа полученного из Multibank
    /// </summary>
    /// <param name="signature_information">Токен из JSon мультибанка(data.signature_information)</param>
    /// <param name="document"></param>
    public virtual void RegistrSigning(Newtonsoft.Json.Linq.JObject signature_information, Sungero.Content.IElectronicDocument document)
    {
      if (signature_information.SelectToken("contragent").ToString() != "" && document.HasVersions)
      {
        JArray signaturesJson = JArray.Parse(JObject.Parse(this.GetSignatures(MultibankSolution.IncomingTaxInvoices.Get(document.Id).Guidmicros)).SelectToken("data").ToString());
        byte[] samDocument = this.GetAttachedBodyFromSignature(Convert.FromBase64String(signaturesJson.First.SelectToken("signature_pkcs7").ToString()));
        MemoryStream stream = new MemoryStream(samDocument);
        document.LastVersion.Body.Write(stream);
        document.Save();
        foreach (JObject sign in signaturesJson)
        {
          string s = sign.SelectToken("signature_data").ToString();
          string d = s.Substring(s.IndexOf("person_full_name") + 19);
          string utf8 = d.Substring(0, d.IndexOf('"'));
          byte[] signData = this.ConverSignatureToDeattached(Convert.FromBase64String(sign.SelectToken("signature_pkcs7").ToString()));
          string signString = Encoding.Default.GetString(signData);
          string signatoryFullName =  System.Text.RegularExpressions.Regex.Replace(utf8, @"\\u([0-9A-Fa-f]{4})", m => ""+(char)Convert.ToInt32(m.Groups[1].Value, 16));
          Logger.Debug("signatoryFullName: " + signatoryFullName);
          int i = 1;
          foreach (var sgn in this.SplitSignatures(signData))
          {
            Signatures.Import(document, SignatureType.Approval, signatoryFullName, sgn, document.LastVersion);
            i++;
          }
        }
        document.Save();
      }
    }
    
    /// <summary>
    /// Получить все подписи по документу из Мультибанка
    /// </summary>
    /// <param name="document_id"></param>
    public virtual string GetSignatures(string document_id)
    {
      var client = new RestClient(this.GetIntegrationDataBookForCurrentUser().ExchangeService.GetSignaturesmicros);
      //string token = this.GetMultibankToken();
      client.Authenticator = new HttpBasicAuthenticator("micros24site","a8lo23d0r3f48");
      var request = new RestRequest(Method.POST);
      request.AddParameter("document_id", document_id);
      IRestResponse response = client.Execute(request);
      Logger.Debug(response.Content);
      return response.Content;
    }
    
    /// <summary>
    /// Получить приложенное к подписи тело документа
    /// </summary>
    /// <param name="signature"></param>
    /// <returns></returns>
    public byte[] GetAttachedBodyFromSignature(byte[] signature)
    {
      var asn1Object = Asn1Object.FromByteArray(signature);
      var contentInfo = ContentInfo.GetInstance(asn1Object);
      var signedData = SignedData.GetInstance(contentInfo.Content);
      return ((Asn1OctetString)signedData.EncapContentInfo.Content).GetOctets();
    }
    
    /// <summary>
    /// Разъединить подписи
    /// </summary>
    /// <param name="signature"></param>
    /// <returns></returns>
    public System.Collections.Generic.IEnumerable<byte[]> SplitSignatures(byte[] signature)
    {
      var result = new List<byte[]>();

      var signedData = SignedData.GetInstance(ContentInfo.GetInstance(Asn1Object.FromByteArray(signature)).Content);
      // Эти данные могут быть задублированы если подписей несколько, RX-у от этого нехорошо, поэтому удаляем дубли.
      var digestAlgorithms = signedData.DigestAlgorithms != null ? new DerSet(signedData.DigestAlgorithms.OfType<Asn1Encodable>().Distinct().ToArray()) : null;
      var certificates = signedData.Certificates != null ? new DerSet(signedData.Certificates.OfType<Asn1Encodable>().Distinct().ToArray()) : null;
      var crls = signedData.CRLs != null ? new DerSet(signedData.CRLs.OfType<Asn1Encodable>().Distinct().ToArray()) : null;

      foreach(var signerInfo in signedData.SignerInfos.ToArray())
      {
        var newSignedData = new SignedData(digestAlgorithms, signedData.EncapContentInfo, certificates, crls, new DerSet(signerInfo));
        result.Add(new ContentInfo(CmsObjectIdentifiers.SignedData, newSignedData).GetDerEncoded());
      }

      return result;
    }
    
    /// <summary>
    /// Получить из НЕ публичного тела документа json, если документ был загружен в виде json
    /// </summary>
    /// <param name="document">Документ</param>
    /// <returns>string json</returns>
    [Public, Remote]
    public virtual string GetJsonFromBody(Sungero.Content.IElectronicDocument document)
    {
      string json = "";
      using (var sr = new StreamReader(document.LastVersion.Body.Read()))
        using (var jsonTextReader = new JsonTextReader(sr))
          json = new JsonSerializer().Deserialize(jsonTextReader).ToString();
      return json;
    }
    
    /// <summary>
    /// Получить document_password_owner из json в виде string
    /// </summary>
    /// <param name="json">json документ в виде строки</param>
    /// <returns>document_password_owner в виде строки</returns>
    [Public, Remote]
    public virtual string GetJsonMultibankPassword(string json)
    {
      string password = JObject.Parse(json).SelectToken("document.document_password_owner").ToString();
      return password;
    }
    
    /// <summary>
    /// Получить document_id из json в виде string
    /// </summary>
    /// <param name="json">json документ в виде строки</param>
    /// <returns>document_id в виде строки</returns>
    [Public, Remote]
    public virtual string GetJsonMultibankGuid(string json)
    {
      string guid = JObject.Parse(json).SelectToken("document.document_id").ToString();
      return guid;
    }
    
    /// <summary>
    /// Получить новый json из Multibank по текущему документу
    /// </summary>
    /// <param name="document">Документ</param>
    /// <returns>string json</returns>
    [Public, Remote]
    public virtual string GetMultibankJsonFromID(Sungero.Content.IElectronicDocument document)
    {
      string document_id = MultibankSolution.IncomingTaxInvoices.Get(document.Id).Guidmicros;
      var bAuth = micros.MultibankModule.AuthDatabooks.GetAll().Where(x => x.BusinessUnit == Sungero.Company.Employees.Current.Department.BusinessUnit).ToList();
      var client = new RestClient(this.GetIntegrationDataBookForCurrentUser().ExchangeService.GetPublicDocmicros + document_id);
      client.Authenticator = new JwtAuthenticator(this.GetMultibankToken());
      var request = new RestRequest(Method.GET);
      request.AddParameter("params","public");
      IRestResponse response = client.Execute(request);
      JObject json = JObject.Parse(response.Content.ToString());
      return json.SelectToken("data").ToString();
      //return response.Content.ToString();
    }
    

    
    /// <summary>
    /// Получить статус документа из json
    /// </summary>
    /// <param name="transaction_operation">параметр в json</param>
    /// <returns>Статус</returns>
    public string GetStatusFromMultibank(string transaction_operation)
    {
      if (transaction_operation == "CreateByOwner" || transaction_operation == "RevokedBySeller" || transaction_operation == "RevokedForBuyer")
        return "Draft";
      else if (transaction_operation == "RejectBySender" || transaction_operation == "RejectByRecipient" || transaction_operation == "CanceledByOwner" || transaction_operation == "CanceledByRecipient" || transaction_operation == "RejectByAgent")
        return "Obsolete";
      else if (transaction_operation == "SignedByOwner" || transaction_operation == "SentByOwner" || transaction_operation == "ReceivedByRecipient" || transaction_operation == "AcceptBySender" || transaction_operation == "AcceptByRecipient" || transaction_operation == "OneSidedInvoiceSent" || transaction_operation == "AcceptByAgent")
        return "Active";
      else return null;
    }
    
    /// <summary>
    /// Получить токен Мультибанка
    /// </summary>
    /// <returns>ответ Мультибанка содержащий токен</returns>
    public string GetMultibankToken()
    {
      var bAuth = micros.MultibankModule.AuthDatabooks.GetAll().Where(x => x.BusinessUnit == Sungero.Company.Employees.Current.Department.BusinessUnit).FirstOrDefault();
      string token = " ";
      if (Convert.ToInt32(bAuth.ExpireAccess) > DateTimeOffset.Now.ToUnixTimeSeconds())
      {
        token = Encoding.Default.GetString(bAuth.AccessToken);
        return token;
      }
      else
      {
        var authD = bAuth;
        //string clientId = authD.
        //var respond = Multibank.PublicFunctions.Module.AuthWithLogin(authD.ClientId, authD.ClientSecret, authD.Login, authD.Password);
        MultibankModule.PublicFunctions.AuthDatabook.UpdateTokens(bAuth, MultibankModule.PublicFunctions.Module.RefreshToken(Encoding.Default.GetString(bAuth.RefreshToken)));
        return Encoding.Default.GetString(bAuth.AccessToken);
      }
      
    }
    
    /// <summary>
    /// Соединить все подписи из документа
    /// </summary>
    /// <param name="signature1"></param>
    /// <param name="signature2"></param>
    /// <returns></returns>
    public byte[] MergeSignatures(byte[] signature1, byte[] signature2)
    {
      var digestAlgorithms = new List<Asn1Encodable>();
      var certificates = new List<Asn1Encodable>();
      var crls = new List<Asn1Encodable>();
      var signers = new List<Asn1Encodable>();

      var signedData1 = SignedData.GetInstance(ContentInfo.GetInstance(Asn1Object.FromByteArray(signature1)).Content);

      if (signedData1.DigestAlgorithms != null)
        digestAlgorithms.AddRange(signedData1.DigestAlgorithms.ToArray());
      if (signedData1.Certificates != null)
        certificates.AddRange(signedData1.Certificates.ToArray());
      if (signedData1.CRLs != null)
        crls.AddRange(signedData1.CRLs.ToArray());
      if (signedData1.SignerInfos != null)
        signers.AddRange(signedData1.SignerInfos.ToArray());

      var signedData2 = SignedData.GetInstance(ContentInfo.GetInstance(Asn1Object.FromByteArray(signature2)).Content);

      if (signedData2.DigestAlgorithms != null)
        digestAlgorithms.AddRange(signedData2.DigestAlgorithms.ToArray());
      if (signedData2.Certificates != null)
        certificates.AddRange(signedData2.Certificates.ToArray());
      if (signedData2.CRLs != null)
        crls.AddRange(signedData2.CRLs.ToArray());
      if (signedData2.SignerInfos != null)
        signers.AddRange(signedData2.SignerInfos.ToArray());


      var newSignedData = new SignedData(digestAlgorithms.Any() ? new DerSet(digestAlgorithms.Distinct().ToArray()) : null,
                                         signedData1.EncapContentInfo,
                                         certificates.Any() ? new DerSet(certificates.Distinct().ToArray()) : null,
                                         crls.Any() ? new DerSet(crls.Distinct().ToArray()) : null,
                                         signers.Any() ? new DerSet(signers.Distinct().ToArray()) : null);

      return new ContentInfo(CmsObjectIdentifiers.SignedData, newSignedData).GetDerEncoded();
    }
    
    /// <summary>
    /// Сконвертировать открепленную подпись в прикрепленную.
    /// </summary>
    public byte[] ConverSignatureToAttached(byte[] signature, byte[] data)
    {
      var asn1Object = Asn1Object.FromByteArray(signature);
      var contentInfo = ContentInfo.GetInstance(asn1Object);
      var signedData = SignedData.GetInstance(contentInfo.Content);
      var newEncapContentInfo = new ContentInfo(CmsObjectIdentifiers.Data, new BerOctetString(data));
      var newSignedData = new SignedData(signedData.DigestAlgorithms, newEncapContentInfo, signedData.Certificates, signedData.CRLs, signedData.SignerInfos);
      var newContentInfo = new ContentInfo(contentInfo.ContentType, newSignedData);
      return newContentInfo.GetDerEncoded();
    }
    #endregion
    
    #region Отказ от AuthDataBook
    
    /// <summary>
    /// Обновить токены из Мультибанка
    /// </summary>
    /// <param name="response">ответ Мультибанка содержащий токены авторизации</param>
    /// <param name="box">Абонентский ящик нашей организации</param>
    [Public, Remote]
    public void UpdateTokens(string response, micros.MultibankSolution.IBusinessUnitBox box)
    {
      JObject json = JObject.Parse(response);
      
      //Сохранить Токен доступа и токен обновлений в БД(свойство справочника) в виде бинарных данных, и сохранение срока действие токена в виде стринг(Формат даты Linux Time)
      box.AccessTokenmicros = Encoding.Default.GetBytes(json.SelectToken("access_token").ToString());
      box.RefreshTokenmicros = Encoding.Default.GetBytes(json.SelectToken("refresh_token").ToString());
      //JObject json2 = JsonConvert.DeserializeObject<JObject>(Encoding.UTF8.GetString(Convert.FromBase64String(json.SelectToken("access_token").ToString().Split('.')[1] + "=")));
      string access_token = json.SelectToken("access_token").ToString().Split('.')[1] + "=";
      Logger.Debug("Access_Tokent = " + access_token);
      var accessTokenByte = Convert.FromBase64String(access_token);
      string accessTokenString = Encoding.UTF8.GetString(accessTokenByte);
      JObject json2 = JsonConvert.DeserializeObject<JObject>(accessTokenString);
      box.ExpireAccessmicros = json2.SelectToken("exp").ToString();
      box.Save();
    }
    
    /// <summary>
    /// Создать профили из Мультибанка привязанные к аккаунту мультибанка.
    /// </summary>
    /// <param name="box"></param>
    [Public, Remote]
    public void CreateProfileList(micros.MultibankSolution.IBusinessUnitBox box)
    {
      //В случае если токен уже недействителен
      
      JObject json = JObject.Parse(MultibankModule.PublicFunctions.Module.GetProfile(Encoding.Default.GetString(box.AccessTokenmicros)));
      
      int count = json.SelectToken("data").Count();
      
      for(int i = 0; i <= count - 1; i++)
      {
        if (!MultibankModule.ProfilesLists.GetAll().Any(a => a.ProfileID == json.SelectToken("data.["+ i +"].profile_id").ToString()))
        {
          var book = MultibankModule.ProfilesLists.Create();
          book.Login = box.Login;
          book.ProfileID = json.SelectToken("data.["+ i +"].profile_id").ToString();
          book.Name = json.SelectToken("data.["+ i +"].name").ToString();
          book.UserId = json.SelectToken("data.["+ i +"].user_id").ToString();
          book.Save();
        }
      }
      #endregion
      
      #endregion
      
    }
    
    #region Utilits
    
    /// <summary>
    /// Обновить токен если время жизни токена истекло
    /// </summary>
    /// <param name="box">Абонентский ящик нашей организации</param>
    public void IfNeedRefreshTokens(micros.MultibankSolution.IBusinessUnitBox box)
    {
      if (DateTimeOffset.FromUnixTimeSeconds(long.Parse(box.ExpireAccessmicros)) <= Calendar.Now)
      {
        var isSTG = false;
        if (box.ExchangeService.ExchangeProvider == micros.MultibankSolution.ExchangeService.ExchangeProvider.MultibankSTGmicros) isSTG = true;
        var respond = MultibankModule.PublicFunctions.Module.AuthWithLogin(box.ClientIdmicros.ToString(), box.ClientSecretmicros, box.Login, box.Password, isSTG);  // Запрос на получение токена
        micros.MultibankModule.PublicFunctions.Module.Remote.UpdateTokens(respond, box);  // Вызов метода для сохранение токенов в БД.
        //if(box.MultibankCompanymicros != null)
          //MultibankModule.PublicFunctions.Module.JoinProfile(Encoding.Default.GetString(box.AccessTokenmicros), box.MultibankCompanymicros.ProfileID);
        box.Save();
      }
    }
    #endregion
  }
}