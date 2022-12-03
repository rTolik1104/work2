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
using Org.BouncyCastle.X509.Store;


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

    private string Base64DecodeObject(string base64String)
    {
      var base64EncodedBytes = Convert.FromBase64String(base64String);
      string json = Encoding.UTF8.GetString(base64EncodedBytes);
      return json;
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
    /// <param name="box">Абонентский ящик Орг.</param>
    /// <returns>PDF в Base64, если PDF документ был не получен, возвращает текст - "False"</returns>
    public string GetPDF(Newtonsoft.Json.Linq.JObject json, micros.MultibankSolution.IBusinessUnitBox box)
    {
      var client = new RestClient(box.ExchangeService.GetPDFmicros);
      var request = new RestRequest(Method.POST);
      request.AddParameter("is_new", "false");
      request.AddParameter("document_id", json.SelectToken("document.document_id").ToString());
      request.AddParameter("password", json.SelectToken("document.document_password_owner").ToString());
      IRestResponse response = client.Execute(request);
      return response.IsSuccessful ? JObject.Parse(response.Content).SelectToken("data").ToString() : JObject.Parse(response.Content).SelectToken("success").ToString();
    }
    
    /// <summary> Создание документа в Мультибанке. </summary>
    /// <param name="jsonString">Json для отправки в Мультибанк</param>
    /// <param name="token">Токен доступа</param>
    /// <returns>Ответ в виде string</returns>
    [Public, Remote]
    public string SendDocument(string docType, string jsonString, IEContract contract)
    {
      //JObject json = JObject.Parse(jsonString);
      var box = this.GetBuisnesUnitBoxForCompany(contract.BusinessUnit);
      var client = new RestClient(box.ExchangeService.SendDocumentmicros);
      client.Authenticator = new JwtAuthenticator(this.GetMultibankToken(contract.BusinessUnit));
      var request = new RestRequest(Method.POST);
      request.AlwaysMultipartFormData = true;
      request.AddParameter("doctype_id", docType);
      request.AddParameter("data", jsonString);
      IRestResponse response = client.Execute(request);
      return response.Content;
    }
    
    /// <summary> Получение токена доступа, обновлений и срок действие </summary>
    /// <param name="clientId">Идентификатор приложения запрашивающего токен</param>
    /// <param name="clientSecret">Ключ приложения запрашивающего токен</param>
    /// <param name="userName">Логин пользователя</param>
    /// <param name="password">Пароль пользователя</param>
    /// <param name="staging">Определения сервера (продакшен или тестовый)</param>
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
      Logger.Debug("AuthWithLogin: " + response.Content);
      
      return response.Content;
    }
    
    
    /// <summary>
    /// Метод получения списка доступных для авторизации профилей юридических и физических лиц
    /// </summary>
    /// <param name="box">Абонентский ящик орг.</param>
    /// <returns></returns>
    [Public]
    public string GetProfile(micros.MultibankSolution.IBusinessUnitBox box)
    {
      var client = new RestClient(box.ExchangeService.GetProfilemicros);
      client.Authenticator = new JwtAuthenticator(Encoding.Default.GetString(box.AccessTokenmicros));
      var request = new RestRequest(Method.GET);
      request.AlwaysMultipartFormData = true;
      IRestResponse response = client.Execute(request);
      return response.Content;
    }
    
    /// <summary>
    /// Метод необходим для присоединения профиля компании или же физического лица к токену.
    /// </summary>
    /// <param name="box">Абонентский ящик Орг. </param>
    /// <returns></returns>
    [Public]
    public string JoinProfile(MultibankSolution.IBusinessUnitBox box)
    {
      var client = new RestClient(box.ExchangeService.JoinProfilemicros);
      string token = Encoding.Default.GetString(box.AccessTokenmicros);
      string token64 = Convert.ToBase64String(box.AccessTokenmicros);
      client.Authenticator = new JwtAuthenticator(token);
      var request = new RestRequest(Method.POST);
      request.AlwaysMultipartFormData = true;
      request.AddParameter("profile_id", box.MultibankCompanymicros.ProfileID);
      IRestResponse response = client.Execute(request);
      Logger.Debug("JoinProfile: " + response.Content);
      return response.Content;
    }
    
    #endregion
    
    #region API CRUD
    
    /// <summary>
    /// Общая функция создания и обновления документа
    /// </summary>
    /// <param name="parameter">Json закодированный в Base64</param>
    /// <returns>Метод отработан</returns>
    [Public(WebApiRequestType = RequestType.Post), Remote]
    public virtual string CreateDocument(string parameter)
    {
      var json = JObject.Parse(JsonConvert.DeserializeObject<JObject>(Base64DecodeObject(parameter)).ToString().ToLower());
      
      // Создание документа по типу
      string doctype = json["document"]["doctype_id"].ToString();
      if(doctype == Constants.Module.DoctypeId.Invoice)
        return Create_Invoice(json);
      else if(doctype == Constants.Module.DoctypeId.Actum)
        return Create_ContractStatement(json);
      else if(doctype == Constants.Module.DoctypeId.GnkContract)
        return Create_Contract(json);
      else if(doctype == Constants.Module.DoctypeId.Contract)
        return Create_Contract(json);
      
      return "Неверный тип документа";
    }

    #endregion
    
    #region Документы
    
    #region Создание и обновление "Счет фактуры"
    /// <summary> Создание документа "Счет-фактура полученный" - (IncomingTaxInvoice) </summary>
    /// <param name="json">Json по которому будет создан документ</param>
    /// <returns>Ответ об успехе</returns>
    public virtual string Create_Invoice(Newtonsoft.Json.Linq.JObject json)
    {
      string ownerTin = json.SelectToken("owner.owner_tin").ToString();
      string tr = json.SelectToken("transaction.transaction_operation").ToString();  //Операция транзакции
      bool isIncoming = true;
      
      //Проверка - входищий ли это документ или исходящий
      if (tr.Contains("owner") || tr.Contains("sender") || tr.Contains("seller")) isIncoming = false;
      
      IIntegrationDatabook databook = IntegrationDatabooks.GetAll().Where(x => x.Guid == json.SelectToken("document.document_id").ToString() && x.IsIncoming == isIncoming).FirstOrDefault();
      if (databook == null)
      {
        databook = IntegrationDatabooks.Create();
        databook.IsIncoming = isIncoming;
        databook.JSon = Encoding.Default.GetBytes(json.ToString());
        databook.Guid = json.SelectToken("document.document_id").ToString();
        Sungero.Docflow.IAccountingDocumentBase document;
        databook.JSon = Encoding.Default.GetBytes(json.ToString());
        if (isIncoming) document = Sungero.FinancialArchive.IncomingTaxInvoices.Create();
        else document = Sungero.FinancialArchive.OutgoingTaxInvoices.Create();
        document.Save();
        databook.Document = document;

        databook.Save();
        this.UpdateTaxInvoice(json.ToString(), databook);
        databook.Name = document.Name;
        databook.Save();
        
        if(isIncoming) this.CreateTask(document, databook);
        
        return "OK - " + databook.Document.Info.ToString();
      }
      else
      {
        //Обновить запись
        databook.JSon = Encoding.Default.GetBytes(json.ToString());
        this.UpdateTaxInvoice(json.ToString(), databook);
        return "OK - " + databook.Document.Info.ToString();
      }
    }
    
    /// <summary>
    /// Обновить СФ из Json
    /// </summary>
    /// <param name="jsonString">Данные из Multibank</param>
    /// <param name="databook">Справочник интеграции</param>
    [Public, Remote]
    public virtual void UpdateTaxInvoice(string jsonString, IIntegrationDatabook databook)
    {
      var document = Sungero.Docflow.AccountingDocumentBases.As(databook.Document);
      JObject json = JObject.Parse(jsonString);
      JToken goods = json.SelectToken("document.goods");
      
      document.Currency = Sungero.Commons.Currencies.GetAll().Where(x => x.AlphaCode == "UZS").FirstOrDefault();
      
      document.TotalAmount = this.GetSum(json.SelectToken("document.goods").ToString(), true);
      document.Subject = GetSubject(json.SelectToken("document.goods").ToString());
      document.BusinessUnit = this.GetBusnesUnitByTin(json, databook.IsIncoming.Value);
      document.Counterparty = this.GetOrCreatCounterpartyeByTin(json, databook.IsIncoming.Value);
      if (micros.DrxUzbekistan.Companies.Is(document.Counterparty))
        document.CounterpartySignatory = this.Get_Signer(json, databook.IsIncoming.Value, micros.DrxUzbekistan.Companies.As(document.Counterparty));
      document.RegistrationNumber = json.SelectToken("document.document_number").ToString();
      document.RegistrationDate = Convert.ToDateTime(json.SelectToken("document.document_date").ToString());
      
      this.IfNeedRefreshTokens(this.GetBuisnesUnitBoxForCompany(document.BusinessUnit));

      //Проверка на подписание
      if(this.IsSignedInMultibank(json)) databook.Sign = Convert.FromBase64String(this.GNKString(databook, "accept", string.Empty));
      databook.Save();
      
      //Создание версии документа
      this.VerifyOrCreateVersion(databook);
      
      string status = json.SelectToken("transaction.transaction_operation").ToString();
      if(status == "CreateByOwner")
      {
        document.LifeCycleState = MultibankSolution.IncomingTaxInvoice.LifeCycleState.Draft;
      }
      else document.LifeCycleState = document.LifeCycleStateAllowedItems.Where(n => n.Value.ToLower().StartsWith(status.Substring(0, 11))).FirstOrDefault();
      
      document.Save();
      this.RegistrSigning(databook);
      
    }
    #endregion
    
    
    #region Создание и обновление "АКТ выполненых работ"
    
    /// <summary> Создание документа "АКТ выполненых работ" - (IncomingTaxInvoice) </summary>
    /// <param name="json">Json по которому будет создан документ</param>
    /// <returns>Ответ об успехе</returns>
    public virtual string Create_ContractStatement(Newtonsoft.Json.Linq.JObject json)
    {
      string ownerTin = json.SelectToken("owner.owner_tin").ToString();
      string tr = json.SelectToken("transaction.transaction_operation").ToString();  //Операция транзакции
      string companyTin = String.Empty;
      bool isIncoming;
      if (tr.Contains("owner") || tr.Contains("sender") || tr.Contains("seller"))
      {
        companyTin = ownerTin;
        isIncoming = false;
      }
      else
      {
        companyTin = json.SelectToken("contragent.contragent_tin").ToString();
        isIncoming = true;
      }
      var databook = IntegrationDatabooks.GetAll().Where(x => x.Guid == json.SelectToken("document.document_id").ToString() && x.IsIncoming == isIncoming).FirstOrDefault();
      var company = Sungero.Company.BusinessUnits.GetAll().Where(x => x.TIN == companyTin).FirstOrDefault();
      var box = this.GetBuisnesUnitBoxForCompany(company);
      this.IfNeedRefreshTokens(box);
      if(databook == null)
      {
        databook = IntegrationDatabooks.Create();
        databook.JSon = Encoding.Default.GetBytes(json.ToString());
        databook.Guid = json.SelectToken("document.document_id").ToString();
        databook.IsIncoming = isIncoming;
        var document = Sungero.FinancialArchive.ContractStatements.Create();
        document.Save();
        databook.Document = document;
        databook.Save();
        this.UpdateContractStatementFromJson(json.ToString(), document, isIncoming);

        databook.Name = document.Name;
        databook.Save();

        if(isIncoming) this.CreateTask(document, databook);
        
        return "OK - " + document.Info.ToString();
      }
      else
      {
        databook.JSon = Encoding.Default.GetBytes(json.ToString());
        databook.Save();
        this.UpdateContractStatementFromJson(json.ToString(), databook.Document, isIncoming);
        return "OK - " + databook.Document.Info.ToString();
      }
    }
    
    /// <summary>
    /// Обновить "АКТ выполненых работ" из json
    /// </summary>
    /// <param name="jsonString">новый json</param>
    /// <param name="documentOld">старый документ</param>
    [Public, Remote]
    public virtual void UpdateContractStatementFromJson(string jsonString, Sungero.Docflow.IOfficialDocument documentOLD, bool isIncoming)
    {
      var document = MultibankSolution.ContractStatements.Get(documentOLD.Id);
      JObject json = JObject.Parse(jsonString);
      string ownerTin = json.SelectToken("owner.owner_tin").ToString();
      string clientTin = json.SelectToken("contragent.contragent_tin").ToString();
      
      document.Currency = Sungero.Commons.Currencies.GetAll().Where(x => x.AlphaCode == "UZS").FirstOrDefault();
      
      //TODO: Вынести метод вычисления суммы
      
      document.TotalAmount = this.GetSum(json.SelectToken("document.goods").ToString(), false);;
      document.BusinessUnit = this.GetBusnesUnitByTin(json, isIncoming);
      document.Counterparty = this.GetOrCreatCounterpartyeByTin(json, isIncoming);
      if (micros.DrxUzbekistan.Companies.Is(document.Counterparty))
        document.CounterpartySignatory = this.Get_Signer(json, isIncoming, micros.DrxUzbekistan.Companies.As(document.Counterparty));
      
      document.Subject = GetSubject(json.SelectToken("document.goods").ToString());
      document.RegistrationNumber = json.SelectToken("document.document_number").ToString();
      document.RegistrationDate = Convert.ToDateTime(json.SelectToken("document.document_date").ToString());
      //  Если свойства Наша организация или Контрагент пустая, создать новую на основе данных из json

      //Проверка на подписание
      var databook = IntegrationDatabooks.GetAll().Where(x => x.Guid == json.SelectToken("document.document_id").ToString() && x.IsIncoming == isIncoming).FirstOrDefault();
      if(this.IsSignedInMultibank(json)) databook.Sign = Convert.FromBase64String(this.GNKString(databook, "accept", string.Empty));
      databook.Save();
      
      //Создание версии документа
      this.VerifyOrCreateVersion(databook);
      
      string status = json.SelectToken("transaction.transaction_operation").ToString();
      if(status == "CreateByOwner")
      {
        document.LifeCycleState = MultibankSolution.IncomingTaxInvoice.LifeCycleState.Draft;
      }
      else document.LifeCycleState = document.LifeCycleStateAllowedItems.Where(n => n.Value.ToLower().StartsWith(status.Substring(0, 11))).FirstOrDefault();
      
      document.Save();
      this.RegistrSigning(databook);
      
    }
    
    #endregion
    
    #region Создание и обновление "Договоров"
    
    /// <summary>
    /// Создание документа "Договор" - (Contract)
    /// </summary>
    /// <param name="json">Json по которому будет создан документ</param>
    /// <returns>Ответ об успехе</returns>
    public virtual string Create_Contract(Newtonsoft.Json.Linq.JObject json)
    {
      string ownerTin = json.SelectToken("owner.owner_tin").ToString();
      string tr = json.SelectToken("transaction.transaction_operation").ToString();  //Операция транзакции
      string companyTin = String.Empty;
      bool isIncoming;
      if (tr.Contains("owner") || tr.Contains("sender") || tr.Contains("seller"))
      {
        companyTin = ownerTin;
        isIncoming = false;
      }
      else
      {
        companyTin = json.SelectToken("contragent.contragent_tin").ToString();
        isIncoming = true;
      }
      var company = Sungero.Company.BusinessUnits.GetAll().Where(x => x.TIN == companyTin).FirstOrDefault();
      var box = this.GetBuisnesUnitBoxForCompany(company);
      this.IfNeedRefreshTokens(box);
      if(!IntegrationDatabooks.GetAll().Any(x => x.Guid == json.SelectToken("document.document_id").ToString() && x.IsIncoming == isIncoming))
      {
        var databook = IntegrationDatabooks.Create();
        databook.JSon = Encoding.Default.GetBytes(json.ToString());
        databook.Guid = json.SelectToken("document.document_id").ToString();
        databook.IsIncoming = isIncoming;
        var document = EContracts.Create();
        databook.Document = document;
        this.UpdateContractFromJson(json.ToString(), databook);
        databook.Save();
        

        databook.Name = document.Name;
        databook.Save();

        if(isIncoming) this.CreateTask(document, databook);
        
        return "OK - " + document.Info.ToString();
      }
      else
      {
        var databook = IntegrationDatabooks.GetAll().Where(x => x.Guid == json.SelectToken("document.document_id").ToString() && x.IsIncoming == isIncoming).FirstOrDefault();
        databook.JSon = Encoding.Default.GetBytes(json.ToString());
        databook.Save();
        this.UpdateContractFromJson(json.ToString(), databook);
        //var task = ReviewTasks.GetAll().Where(x => x.IntegrationDatabook == databook).FirstOrDefault();
        //if (task != null) this.UpdateDocumentStatusForMBKAssignment(task);
        return "OK - " + databook.Document.Info.ToString();
      }
    }
    
    /// <summary>
    /// Обновить "Договор" из json
    /// </summary>
    /// <param name="jsonString">новый json</param>
    /// <param name="documentOld">старый документ</param>
    [Public, Remote]
    public virtual void UpdateContractFromJson(string jsonString, IIntegrationDatabook databook)
    {
      var document = EContracts.As(databook.Document);
      bool isIncoming = databook.IsIncoming.Value;
      //if (Locks.GetLockInfo(document).IsLocked) Locks.Unlock(document);
      JObject json = JObject.Parse(jsonString);
      string ownerTin = json.SelectToken("owner.owner_tin").ToString();
      string contragentTin = json.SelectToken("contragent.contragent_tin").ToString();

      document.Subject = GetSubject(json.SelectToken("document.goods").ToString());
      document.TotalAmount = this.GetSum(json.SelectToken("document.goods").ToString(), true);
      document.Currency = Sungero.Commons.Currencies.GetAll().Where(x => x.AlphaCode == "UZS").FirstOrDefault();
      document.Title = json.SelectToken("document.contract.contract_name").ToString();
      document.BusinessUnit = this.GetBusnesUnitByTin(json, isIncoming);
      document.ContractAddress = json.SelectToken("document.contract.place").ToString();
      document.ValidFrom = DateTime.Parse(json.SelectToken("document.document_date").ToString());
      document.ValidTill = DateTime.Parse(json.SelectToken("document.contract.expire_date").ToString());
      document.Counterparty = this.GetOrCreatCounterpartyeByTin(json, isIncoming);
      if (micros.DrxUzbekistan.Companies.Is(document.Counterparty))
        document.CounterpartySignatory = this.Get_Signer(json, isIncoming, micros.DrxUzbekistan.Companies.As(document.Counterparty));
      
      //Заполнение условий договора
      var terms = json.SelectToken("document.terms").ToList();
      foreach (var term in terms)
      {
        var t = document.Terms.AddNew();
        t.Number = term.Value<int>("ord_no");
        t.TermTitle = term.Value<string>("term_title");
        t.TermText = term.Value<string>("term_text");
      }
      
      //Заполнение товаров/услуг
      var goods = json.SelectToken("document.goods");
      foreach(var good in goods)
      {
        var g = document.Goods.AddNew();
        g.No = good.Value<int>("no");
        g.Name = good.Value<string>("name");
        g.CatalogCode = good.Value<string>("catalog_code");
        g.CatalogName = good.Value<string>("catalog_name");
        g.Unit = micros.MicrosSetting.Measurements.GetAll(x => x.MeasureId == good.Value<int>("unit")).FirstOrDefault();
        g.Qty = good.Value<double>("qty");
        g.Price = good.Value<double>("price");
        if (!JTokenIsNullOrEmpty(good.SelectToken("vat_percent")))
          g.VatPercent = Convert.ToInt32(good.Value<double>("vat_percent"));
        else g.VatPercent = 0;
        g.VatSum = good.Value<double>("vat_sum");
        g.VatTotalSum = good.Value<double>("vat_total_sum");
        g.TotalSum = good.Value<double>("total_sum");
        g.WithoutVat = good.Value<bool>("without_vat");
        g.Amount = good.Value<double>("amount");
      }
      
      document.RegistrationNumber = json.SelectToken("document.document_number").ToString();
      if (document.Subject == null) document.Subject = GetSubject(json.SelectToken("document.goods").ToString());
      document.RegistrationDate = Convert.ToDateTime(json.SelectToken("document.document_date").ToString());
      //  Если свойства Наша организация или Контрагент пустая, создать новую на основе данных из json

      //Проверка на подписание
      if(this.IsSignedInMultibank(json)) databook.Sign = Convert.FromBase64String(this.GNKString(databook, "accept", string.Empty));
      databook.Save();
      
      //Создание версии документа
      this.VerifyOrCreateVersion(databook);
      
      string status = json.SelectToken("transaction.transaction_operation").ToString();
      if(status == "CreateByOwner")
      {
        document.LifeCycleState = MultibankSolution.IncomingTaxInvoice.LifeCycleState.Draft;
      }
      else document.LifeCycleState = document.LifeCycleStateAllowedItems.Where(n => n.Value.ToLower().StartsWith(status.Substring(0, 11))).FirstOrDefault();
      //var test = document.LifeCycleStateAllowedItems.Where(n => n.Value.ToLower().StartsWith(status.Substring(0, 11))).FirstOrDefault();
      
      document.Save();
      this.RegistrSigning(databook);
    }
    #endregion
    
    #region Работа с версиями и задачами
    /// <summary>
    /// Проверить/Создать версию документа
    /// </summary>
    /// <param name="databook"></param>
    public virtual void VerifyOrCreateVersion(IIntegrationDatabook databook)
    {
      var document = databook.Document;
      Sungero.Content.IElectronicDocumentVersions version;
      bool created = false;
      if (document.HasVersions) version = document.LastVersion;
      else
      {
        document.CreateVersion();
        version = document.LastVersion;
        created = true;
      }
      if (databook.Sign != null)
      {
        var body = this.GetAttachedBodyFromSignature(databook.Sign);
        var stream = document.LastVersion.Body.Read();
        MemoryStream ms = new MemoryStream();
        stream.CopyTo(ms);
        byte[] bodyOld = ms.ToArray();
        //Logger.Debug("BodyOLD: " + Encoding.Default.GetString(bodyOld));
        //Logger.Debug("NewBody: " + Encoding.Default.GetString(body));
        if (Encoding.Default.GetString(bodyOld) != Encoding.Default.GetString(body)) //ППЦ какой бред, но проверка с байтами не работает!
        {
          if (created)
          {
            MemoryStream newBodyStream = new MemoryStream(body);
            version.Body.Write(newBodyStream);
            version.BodyAssociatedApplication = Sungero.Content.AssociatedApplications.GetByExtension("json");
          }
          else
          {
            MemoryStream newBodyStream = new MemoryStream(body);
            document.CreateVersion();
            version = document.LastVersion;
            version.Body.Write(newBodyStream);
            version.BodyAssociatedApplication = Sungero.Content.AssociatedApplications.GetByExtension("json");
            created = true;
          }
        }
      }
      else
      {
        MemoryStream ms = new MemoryStream(databook.JSon);
      }
      string pdfBase64 = GetPDF(JObject.Parse(Encoding.Default.GetString(databook.JSon)), this.GetBuisnesUnitBoxForCompany(document.BusinessUnit));
      if(pdfBase64 != "false" && pdfBase64 != "False" && pdfBase64 != null)
      {
        byte[] pdfByte = Convert.FromBase64String(pdfBase64);
        MemoryStream pdfStream = new MemoryStream(pdfByte);
        version.PublicBody.Write(pdfStream);
        version.AssociatedApplication = Sungero.Content.AssociatedApplications.GetByExtension("pdf");
        //version.Save();
      }
      else
        version.AssociatedApplication = Sungero.Content.AssociatedApplications.GetByExtension("json");
      document.Save();
    }
    
    /// <summary>
    /// Создание задачи "На обработку"
    /// </summary>
    /// <param name="document"></param>
    public virtual void CreateTask(Sungero.Docflow.IOfficialDocument document, IIntegrationDatabook databook)
    {
      var task = micros.MultibankModule.ReviewTasks.Create();
      task.DocumentGroup.OfficialDocuments.Add(document);
      task.IntegrationDatabook = databook;
      string subject = "Получен документ : " + document.Name;
      if (subject.Length > 250) subject = subject.Substring(0, 249); // Макс. длина темы 250 символов, и это изменить нельзя
      task.Subject = subject;
      var box = this.GetBuisnesUnitBoxForCompany(document.BusinessUnit);
      task.Performer = box.Responsible;
      task.Start();
    }
    
    /// <summary>
    /// Назначить статус Документа в задании
    /// </summary>
    /// <param name="task"></param>
    [Public, Remote]
    public virtual Sungero.Core.Enumeration GetDocumentStatusForMBKAssignment(IMBKAssignment assignment)
    {
      var document = assignment.DocumentGroup.OfficialDocuments.SingleOrDefault();
      string documentStatus = document.LifeCycleState.Value.Value.Substring(0, 10).ToLower();
      var statuses = assignment.DocumentStatusAllowedItems.ToList();
      var status = statuses.Where(x => x.Value.ToLower().Contains(documentStatus)).FirstOrDefault();
      return status;
    }
    
    public virtual void UpdateDocumentStatusForMBKAssignment(IReviewTask task)
    {
      var assignment = MBKAssignments.GetAll().Where(x => x.MainTask == task && x.Status == MultibankModule.MBKAssignment.Status.InProcess).FirstOrDefault();
      if (assignment != null)
      {
        assignment.DocumentStatus = this.GetDocumentStatusForMBKAssignment(assignment);
        assignment.Save();
      }
    }
    #endregion
    
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
    
    
    

    #endregion
    
    #region Sign-!DEV!
    
    
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
    [Public, Remote]
    public string GNKString(IIntegrationDatabook databook, string action, string opts)
    {
      var box = this.GetBuisnesUnitBoxForCompany(databook.Document.BusinessUnit);
      var client = new RestClient(box.ExchangeService.GNKStringmicros);
      client.Authenticator = new JwtAuthenticator(this.GetMultibankToken(box.BusinessUnit));
      var request = new RestRequest(Method.POST);
      request.AddParameter("document_id", databook.Guid);
      request.AddParameter("action", action);
      if (!String.IsNullOrEmpty(opts))
        request.AddParameter("opts", "{\"notes\": \"" + opts + "\"}");
      IRestResponse response = client.Execute(request);
      JObject json = JObject.Parse(response.Content.ToString());
      string forSign = json.SelectToken("data.string").ToString();
      //Logger.Debug("Test_pcs7: " + forSign);
      return forSign;
    }
    
    /// <summary>
    /// Отклонить подписанные данные в Multibank
    /// </summary>
    /// <param name="document"></param>
    [Public, Remote]
    public virtual string RejectDocumentInMultibank(IIntegrationDatabook databook, string opts)
    {
      var document = databook.Document;
      var client = new RestClient(this.GetBuisnesUnitBoxForCompany(document.BusinessUnit).ExchangeService.Rejectmicros);
      Logger.Debug("Address: " + this.GetBuisnesUnitBoxForCompany(document.BusinessUnit).ExchangeService.Rejectmicros);
      client.Timeout = -1;
      this.IfNeedRefreshTokens(this.GetBuisnesUnitBoxForCompany(document.BusinessUnit));
      string signature = Convert.ToBase64String(databook.OutgoingData);
      string token = this.GetMultibankToken(document.BusinessUnit);
      string document_id = databook.Guid;
      Logger.Debug("Guid: " + document_id);
      var request = new RestRequest(Method.POST);
      request.AddHeader("Authorization", string.Format("Bearer {0}", token));
      request.AlwaysMultipartFormData = true;
      request.AddParameter("document_id", document_id);
      request.AddParameter("sign", signature);
      request.AddParameter("notes", opts);
      IRestResponse response = client.Execute(request);
      Logger.Debug(response.Content);
      return response.Content;
    }

    /// <summary>
    /// Аннулировать подписанные данные в Multibank
    /// </summary>
    /// <param name="document"></param>
    [Public, Remote]
    public virtual string CancelDocumentInMultibank(IIntegrationDatabook databook)
    {
      var document = databook.Document;
      var client = new RestClient(this.GetBuisnesUnitBoxForCompany(document.BusinessUnit).ExchangeService.Cancelmicros);
      Logger.Debug("Address: " + this.GetBuisnesUnitBoxForCompany(document.BusinessUnit).ExchangeService.Cancelmicros);
      client.Timeout = -1;
      this.IfNeedRefreshTokens(this.GetBuisnesUnitBoxForCompany(document.BusinessUnit));
      string signature = Convert.ToBase64String(databook.OutgoingData);
      string token = this.GetMultibankToken(document.BusinessUnit);
      string document_id = databook.Guid;
      var request = new RestRequest(Method.POST);
      request.AddHeader("Authorization", string.Format("Bearer {0}", token));
      request.AlwaysMultipartFormData = true;
      request.AddParameter("document_id", document_id);
      request.AddParameter("sign", signature);
      IRestResponse response = client.Execute(request);
      Logger.Debug(response.Content);
      return response.Content;
    }
    
    /// <summary>
    /// Возвращает подписанные данные в Multibank
    /// </summary>
    /// <param name="document"></param>
    [Public, Remote]
    public virtual string ReturnSigningDocumentInMultibank(IIntegrationDatabook databook)
    {
      var document = databook.Document;
      string signature = Convert.ToBase64String(databook.OutgoingData);
      var client = new RestClient(this.GetBuisnesUnitBoxForCompany(document.BusinessUnit).ExchangeService.MultibankAcceptmicros);
      string token = this.GetMultibankToken(document.BusinessUnit);
      string document_id = databook.Guid;
      
      var request = new RestRequest(Method.POST);
      request.AddHeader("authorization", "Bearer " + token);
      request.AlwaysMultipartFormData = true;

      request.AddParameter("document_id", document_id);
      request.AddParameter("sign", signature);

      //Logger.Debug("pkcs7: " + signature);
      IRestResponse response = client.Execute(request);
      Logger.Debug(response.Content);
      return response.Content;
    }
    
    [Public, Remote]
    public micros.MultibankSolution.IBusinessUnitBox GetBuisnesUnitBoxForCompany(Sungero.Company.IBusinessUnit company)
    {
      return MultibankSolution.BusinessUnitBoxes.GetAll().Where(x => x.BusinessUnit == company)
        .Where(b => b.ExchangeService.ExchangeProvider == micros.MultibankSolution.ExchangeService.ExchangeProvider.Multibank ||
               b.ExchangeService.ExchangeProvider == micros.MultibankSolution.ExchangeService.ExchangeProvider.MultibankSTGmicros).FirstOrDefault();
    }
    
    [Public, Remote]
    public virtual string SignInMultibank(IIntegrationDatabook databook)
    {
      var document = databook.Document;
      string signature = Convert.ToBase64String(databook.OutgoingData);
      var client = new RestClient(this.GetBuisnesUnitBoxForCompany(document.BusinessUnit).ExchangeService.Signingmicros);
      string token = this.GetMultibankToken(document.BusinessUnit);
      string document_id = databook.Guid;
      
      var request = new RestRequest(Method.POST);
      request.AddHeader("authorization", "Bearer " + token);
      request.AlwaysMultipartFormData = true;

      request.AddParameter("document_id", document_id);
      request.AddParameter("pkcs7", signature);

      //Logger.Debug("pkcs7: " + signature);
      IRestResponse response = client.Execute(request);
      Logger.Debug(response.Content);
      return response.Content;
    }
    
    [Public, Remote]
    public virtual string SendByMultibank(IIntegrationDatabook databook)
    {
      var document = databook.Document;
      var client = new RestClient(this.GetBuisnesUnitBoxForCompany(document.BusinessUnit).ExchangeService.SendDocumentmicros + "/send");
      string token = this.GetMultibankToken(document.BusinessUnit);
      string document_id = databook.Guid;
      
      var request = new RestRequest(Method.POST);
      request.AddHeader("authorization", "Bearer " + token);
      request.AlwaysMultipartFormData = true;

      request.AddParameter("document_id", document_id);

      IRestResponse response = client.Execute(request);
      Logger.Debug(response.Content);
      return response.Content;
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
    /// Импорт подписи после второго подписания в e-imzo
    /// </summary>
    /// <param name="sign">подпись в base64</param>
    [Public(WebApiRequestType = RequestType.Post)]
    public virtual string ImportSign(string externalSign, int document_id)
    {
      //Logger.Debug("PKCS7: " + externalSign);
      byte[] signByteArray = Convert.FromBase64String(externalSign);
      
      var databook = IntegrationDatabooks.GetAll().Where(x => x.Document.Id == document_id).FirstOrDefault();
      databook.OutgoingData = signByteArray;
      databook.Save();
      this.RegistrSigning(databook);
      //      var signForImport = this.SplitSignatures(signByteArray).FirstOrDefault();
      //      Signatures.Import(databook.Document, SignatureType.Approval, Sungero.Company.Employees.Current.Name, signForImport, databook.Document.LastVersion);
      return "OK";
    }
    
    /// <summary>
    /// Смотрит, подписан ли документ в Multibank
    /// </summary>
    /// <param name="docId">ID документа в Multibank</param>
    /// <param name="docPassword">Пароль документа в Multibank</param>
    /// <returns></returns>
    public virtual bool IsSignedInMultibank(Newtonsoft.Json.Linq.JObject json)
    {
      if (json.SelectToken("signature_information.contragent").ToString() == "" && json.SelectToken("signature_information.owner").ToString() == "")
        return false;
      else
      {
        return true;
      }
    }
    
    /// <summary>
    /// Импорт подписи документа полученного из Multibank
    /// </summary>
    /// <param name="signature_information">Токен из JSon мультибанка(data.signature_information)</param>
    /// <param name="document"></param>
    public virtual void RegistrSigning(IIntegrationDatabook databook)
    {
      var document = databook.Document;
      if (databook.Sign != null)
      {
        foreach (var sgn in this.SplitSignatures(databook.Sign))
        {
          if (!Signatures.Get(document.LastVersion).Any(x => x.GetDataSignature() == sgn))
          {
            var detachedSign = this.ConverSignatureToDeattached(sgn);
            
            //Тупой метод вытягивания имени подписующего из подписи, но лучше я не придумал
            var signedData = SignedData.GetInstance(ContentInfo.GetInstance(Asn1Object.FromByteArray(detachedSign)).Content);
            var signersCertificates = signedData.Certificates.ToArray().Last();
            var string1 = signersCertificates.ToString().Split('[');
            string string2 = string1.Where(x => x.StartsWith("2.5.4.3")).Last();
            string name = string2.Substring(string2.IndexOf(",") + 2).Replace("]", "").Replace(", ", "");
            Logger.Debug("Signer name: " + name.ToString());
            
            if (!Signatures.Get(document.LastVersion).Any(x => x.SignatoryFullName == name))
            {
              Signatures.Import(document, SignatureType.Approval, name, sgn, document.LastVersion);
            }
          }
        }
      }
      if (databook.OutgoingData != null)
      {
        foreach (var sgn in this.SplitSignatures(databook.OutgoingData))
        {
          if (!Signatures.Get(document.LastVersion).Any(x => x.GetDataSignature() == sgn))
          {
            var detachedSign = this.ConverSignatureToDeattached(sgn);
            
            //Тупой метод вытягивания имени подписующего из подписи, но лучше я не придумал
            var signedData = SignedData.GetInstance(ContentInfo.GetInstance(Asn1Object.FromByteArray(detachedSign)).Content);
            var signersCertificates = signedData.Certificates.ToArray().Last();
            var string1 = signersCertificates.ToString().Split('[');
            string string2 = string1.Where(x => x.StartsWith("2.5.4.3")).Last();
            string name = string2.Substring(string2.IndexOf(",") + 2).Replace("]", "").Replace(", ", "");
            Logger.Debug("Signer name: " + name.ToString());
            
            if (document.HasVersions)
              if (!Signatures.Get(document.LastVersion).Any(x => x.SignatoryFullName == name))
                Signatures.Import(document, SignatureType.Approval, name, sgn, document.LastVersion);
          }
        }
      }
      document.Save();
    }
    
    /// <summary>
    /// Получить все подписи по документу из Мультибанка
    /// </summary>
    /// <param name="document_id"></param>
    public virtual string GetSignatures(string document_id, Sungero.Company.IBusinessUnit company)
    {
      var box = this.GetBuisnesUnitBoxForCompany(company);
      var client = new RestClient(box.ExchangeService.GetSignaturesmicros);
      //string token = this.GetMultibankToken();
      client.Authenticator = new HttpBasicAuthenticator("micros24site","a8lo23d0r3f48");
      var request = new RestRequest(Method.POST);
      request.AddParameter("document_id", document_id);
      IRestResponse response = client.Execute(request);
      Logger.Debug(response.Content);
      //var dataBook = this.Get
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
      //Logger.Debug("DATA: " + signedData.SignerInfos.ToArray().LastOrDefault().ToString());
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
    public virtual string GetMultibankJsonFromID(Sungero.Docflow.IOfficialDocument document)
    {
      var box = this.GetBuisnesUnitBoxForCompany(document.BusinessUnit);
      this.IfNeedRefreshTokens(box);
      string document_id = IntegrationDatabooks.GetAll().Where(x => x.Document == document).FirstOrDefault().Guid;
      var client = new RestClient(box.ExchangeService.GetPublicDocmicros + document_id);
      client.Authenticator = new JwtAuthenticator(this.GetMultibankToken(document.BusinessUnit));
      var request = new RestRequest(Method.GET);
      request.AddParameter("params","public");
      IRestResponse response = client.Execute(request);
      Logger.Debug("Update data: " + response.Content);
      JObject json = JObject.Parse(response.Content);
      return json.SelectToken("data").ToString();
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
    public string GetMultibankToken(Sungero.Company.IBusinessUnit company)
    {
      var bAuth = this.GetBuisnesUnitBoxForCompany(company);
      string token = " ";
      if (Convert.ToInt32(bAuth.ExpireAccessmicros) > DateTimeOffset.Now.ToUnixTimeSeconds())
      {
        token = Encoding.Default.GetString(bAuth.AccessTokenmicros);
        return token;
      }
      else
      {
        var authD = bAuth;
        this.IfNeedRefreshTokens(bAuth);
        return Encoding.Default.GetString(bAuth.AccessTokenmicros);
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
      string access_token = json.SelectToken("access_token").ToString();
      string access_token64 = access_token.Split('.')[1] + "=";
      box.AccessTokenmicros = Encoding.Default.GetBytes(access_token);
      box.RefreshTokenmicros = Encoding.Default.GetBytes(json.SelectToken("refresh_token").ToString());
      
      Logger.Debug("Access_Tokent = " + access_token);
      var accessTokenByte = Convert.FromBase64String(access_token64);
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
      
      JObject json = JObject.Parse(MultibankModule.PublicFunctions.Module.GetProfile(box));
      
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
    }
    #endregion
    
    #endregion
    
    
    
    #region Utilits
    
    /// <summary>
    /// Обновить токен если время жизни токена истекло
    /// </summary>
    /// <param name="box">Абонентский ящик нашей организации</param>
    [Public, Remote]
    public void IfNeedRefreshTokens(micros.MultibankSolution.IBusinessUnitBox box)
    {
      if (DateTimeOffset.FromUnixTimeSeconds(long.Parse(box.ExpireAccessmicros)) <= Calendar.Now)
      {
        var isSTG = false;
        if (box.ExchangeService.ExchangeProvider == micros.MultibankSolution.ExchangeService.ExchangeProvider.MultibankSTGmicros) isSTG = true;
        var respond = MultibankModule.PublicFunctions.Module.AuthWithLogin(box.ClientIdmicros.ToString(), box.ClientSecretmicros, box.Login, box.Password, isSTG);  // Запрос на получение токена
        micros.MultibankModule.PublicFunctions.Module.Remote.UpdateTokens(respond, box);  // Вызов метода для сохранение токенов в БД.
        MultibankModule.PublicFunctions.Module.JoinProfile(box);
        box.Save();
      }
    }
    
    /// <summary>
    /// Костыль для проверки пароля пользователя
    /// </summary>
    /// <returns></returns>
    [Public(WebApiRequestType = RequestType.Get)]
    public virtual string CheckPassword()
    {
      return "Ok";
    }
    
    /// <summary>
    /// Получить строку ошибки из ответа сервера Multibank
    /// </summary>
    /// <param name="response">ответ сервера Multibank</param>
    /// <returns>Error RU</returns>
    [Public, Remote]
    public virtual string GetMessageFromResponse(string response)
    {
      JObject error = JObject.Parse(response);
      return error.SelectToken("message.ru").ToString(Formatting.None);
    }
    
    /// <summary>
    /// Вычисления суммы
    /// </summary>
    /// <param name="jsonArray">Товары из json</param>
    /// <returns></returns>
    public double GetSum(string jsonArray, bool totalSum)
    {
      JArray json = JArray.Parse(jsonArray);
      string route = String.Empty;
      if (totalSum) route = "total_sum";
      else route = "amount";
      double amount = 0;
      foreach(var summ in json)
      {
        amount = amount + Convert.ToDouble(summ.SelectToken(route));
      }
      return amount;
    }
    
    public string GetSubject(string jsonArray)
    {
      JArray json = JArray.Parse(jsonArray);
      string route = "name";
      string subject = String.Empty;
      int i = 1;
      foreach(var s in json)
      {
        subject = subject + s.SelectToken(route);
        if (i < json.Count)
        {
          subject = subject + ", ";
          i++;
        }
      }
      int b = subject.Length;
      if (b > 250) subject = subject.Substring(0, 249);
      return subject;
    }
    
    /// <summary>
    /// Найти компанию по ИНН в json
    /// </summary>
    /// <param name="json">json документа</param>
    /// <param name="isIncoming">Документ - входящий или нет</param>
    /// <returns>Нашу организацию</returns>
    public Sungero.Company.IBusinessUnit GetBusnesUnitByTin(Newtonsoft.Json.Linq.JObject json, bool isIncoming)
    {
      string route = String.Empty;
      if(isIncoming) route = "contragent";
      else route = "owner";
      string tin = json.SelectToken($"{route}.{route}_tin").ToString();
      var company = Sungero.Company.BusinessUnits.GetAll(x => x.TIN == tin).FirstOrDefault();
      return company;
    }
    
    /// <summary>
    /// Ишет или создает контрагента
    /// </summary>
    /// <param name="json">json документа</param>
    /// <param name="isIncoming">Документ - входящий или нет</param>
    /// <returns>Контрагент</returns>
    public Sungero.Parties.ICounterparty GetOrCreatCounterpartyeByTin(Newtonsoft.Json.Linq.JObject json, bool isIncoming)
    {
      //Определение переменных
      string route = String.Empty;
      if(isIncoming) route = "owner";
      else route = "contragent";
      string tin = json.SelectToken($"{route}.{route}_tin").ToString();
      string name = json.SelectToken($"{route}.document_{route}_name").ToString();
      bool isPerson = false;
      Sungero.Parties.ICounterparty counterparty;
      
      //Проверка на компанию/персону
      if (tin.Length == 14)
      {
        counterparty = micros.DrxUzbekistan.People.GetAll(x => x.PINImicros == tin).FirstOrDefault();
        isPerson = true;
      }
      else counterparty = micros.DrxUzbekistan.Companies.GetAll(x => x.TIN == tin).FirstOrDefault();
      
      //Проверка, существует ли контрагент в системе
      if (counterparty == null)
      {
        name = json.SelectToken($"{route}.document_{route}_name").ToString();
        if(isPerson)counterparty = Create_Person(name.ToUpper(), tin, null);
        else counterparty = Create_Company(name, tin);
      }
      
      //Обновление контрагента
      JObject jdocument = JObject.Parse(json.SelectToken("document").ToString());
      if (jdocument.ContainsKey(route))
      {
        if (isPerson) UpdatePersonFromJSon(json, route, micros.DrxUzbekistan.People.As(counterparty));
        else UpdateCompanyFromJSon(json, jdocument, route, micros.DrxUzbekistan.Companies.As(counterparty));
      }
      return counterparty;
    }

    public Sungero.Parties.IContact Get_Signer(Newtonsoft.Json.Linq.JObject json, bool isIncoming, micros.DrxUzbekistan.ICompany company)
    {
      Logger.Debug("start Get_Signer()");
      string route = String.Empty;
      if(isIncoming) route = "owner";
      else route = "contragent";
      Logger.Debug("route: " + route);
      Sungero.Parties.IContact contact;
      Logger.Debug("Contragent tin: " + json.SelectToken($"{route}.{route}_tin").ToString());
      if (json.SelectToken($"{route}.{route}_tin").ToString().Length == 14) return null; //Если контрагент физ лицо, то подписующий не нужен
      if (JTokenIsNullOrEmpty(json.SelectToken($"signature_information.{route}"))) //Если нет подписи контрагента то и подписующего со стороны контрагента нет.
      {
        Logger.Debug("signature information is null");
        return null;
      }
      JObject signatoryInformation = JObject.Parse(json.SelectToken($"signature_information.{route}").ToString());
      Logger.Debug("signature_information: " + signatoryInformation.ToString()); //Потом убрать
      string pinfl = String.Empty;
      string tin = String.Empty;
      if(signatoryInformation.ContainsKey("person_pin")) pinfl = signatoryInformation.SelectToken("person_pin").ToString();
      if(signatoryInformation.ContainsKey("person_tin")) tin = signatoryInformation.SelectToken("person_tin").ToString();
      Logger.Debug("contragent PINFL: " + pinfl);
      var signatory = Get_Person(pinfl, tin);
      if (signatory == null) signatory = Create_Person(signatoryInformation.SelectToken("person_full_name").ToString(), pinfl, tin);
      contact = Sungero.Parties.Contacts.GetAll(x => x.Person == signatory && x.Company == company).FirstOrDefault();
      if (contact == null)
      {
        Logger.Debug("Create contact");
        string jobTitle = String.Empty;
        if (!JTokenIsNullOrEmpty(signatoryInformation.SelectToken("person_position"))) jobTitle = signatoryInformation.SelectToken("person_position").ToString();
        contact = Create_Contact(signatory, company, jobTitle);
      }
      Logger.Debug("Signer: " + signatory.DisplayValue);
      Logger.Debug("Contact: " + contact.Name);
      return contact;
    }
    
    /// <summary> Создание сущности "Организация" </summary>
    /// <param name="name">Название</param>
    /// <param name="tin">ИНН</param>
    /// <param name="address">Адрес</param>
    /// <param name="account">Банковский счёт</param>
    /// <param name="vat">Регистрационный код плательщика НДС</param>
    /// <param name="bankMFO">Банк МФО</param>
    /// <returns>Созданный орг.</returns>
    private micros.DrxUzbekistan.ICompany Create_Company(string name, string tin)
    {
      var party = micros.DrxUzbekistan.Companies.Create();
      party.Name = name.ToUpper();
      party.TIN = tin;
      party.Save();
      return party;
    }
    
    private micros.DrxUzbekistan.IPerson Create_Person(string name, string pnfl, string tin)
    {
      Logger.Debug("Start create person");
      Logger.Debug("Person full name: " + name);
      var person = micros.DrxUzbekistan.People.Create();
      var nameArray = name.ToUpper().Split(new char[0], StringSplitOptions.RemoveEmptyEntries);
      Logger.Debug("Person Last name: " + nameArray[0]);
      Logger.Debug("Person First name: " + nameArray[1]);
      Logger.Debug("Person First name: " + nameArray[2]);
      person.LastName = nameArray[0];
      person.FirstName = nameArray[1];
      person.MiddleName = nameArray[2];
      person.PINImicros = pnfl;
      person.TIN = tin;
      person.PINImicros = pnfl;
      Logger.Debug("Person PINFL: " + tin);
      person.Save();
      return person;
    }
    
    private micros.DrxUzbekistan.IPerson Get_Person(string pinfl, string tin)
    {
      micros.DrxUzbekistan.IPerson person = null;
      if(!String.IsNullOrEmpty(pinfl)) person = micros.DrxUzbekistan.People.GetAll(x => x.PINImicros == pinfl).FirstOrDefault();
      else if (!String.IsNullOrEmpty(tin)) person = micros.DrxUzbekistan.People.GetAll(x => x.TIN == tin).FirstOrDefault();
      if (person != null)
      {
        if (String.IsNullOrEmpty(person.TIN) && !String.IsNullOrEmpty(tin)) person.TIN = tin;
        if (String.IsNullOrEmpty(person.PINImicros) && !String.IsNullOrEmpty(pinfl)) person.PINImicros = pinfl;
      }
      return person;
    }
    
    private micros.DrxUzbekistan.ICompany UpdateCompanyFromJSon (Newtonsoft.Json.Linq.JObject json, Newtonsoft.Json.Linq.JObject document, string route, micros.DrxUzbekistan.ICompany company)
    {
      if (!JTokenIsNullOrEmpty(document.SelectToken($"{route}.{route}_name")))
        company.Name = document.SelectToken($"{route}.{route}_name").ToString().ToUpper();
      if (!JTokenIsNullOrEmpty(document.SelectToken($"{route}.{route}_oked")))
        company.OKEDmicros = document.SelectToken($"{route}.{route}_oked").ToString();
      if (!JTokenIsNullOrEmpty(document.SelectToken($"{route}.{route}_account")))
        company.Account = document.SelectToken($"{route}.{route}_account").ToString();
      if (!JTokenIsNullOrEmpty(document.SelectToken($"{route}.{route}_address")))
        company.LegalAddress = document.SelectToken($"{route}.{route}_address").ToString();
      if (!JTokenIsNullOrEmpty(document.SelectToken($"{route}.{route}_work_phone")))
        company.Phones = document.SelectToken($"{route}.{route}_work_phone").ToString();
      if (!JTokenIsNullOrEmpty(document.SelectToken($"{route}.{route}_bank_id")))
        company.Bank = micros.DrxUzbekistan.Banks.GetAll(x => x.BIC == document.SelectToken($"{route}.{route}_bank_id").ToString()).FirstOrDefault();
      if (!JTokenIsNullOrEmpty(document.SelectToken($"{route}.{route}_district_id")))
        company.Region = micros.DrxUzbekistan.Regions.GetAll(x => x.Code == document.SelectToken($"{route}.{route}_district_id").ToString()).FirstOrDefault();
      company.Save();
      return company;
      
    }
    
    private Sungero.Parties.IContact Create_Contact(micros.DrxUzbekistan.IPerson person, micros.DrxUzbekistan.ICompany company, string jobTitle)
    {
      Logger.Debug("Start create contact");
      Logger.Debug("Person last name: " + person.LastName);
      Logger.Debug("Person first name: " + person.FirstName);
      Logger.Debug("Person middleName: " + person.MiddleName);
      var contact = Sungero.Parties.Contacts.Create();
      contact.Name = String.Format("{0} {1} {2}", person.LastName, person.FirstName, person.MiddleName);
      Logger.Debug("Contact name: " + contact.Name);
      contact.Company = company;
      Logger.Debug("company: " + company.Name);
      contact.Person = person;
      contact.JobTitle = jobTitle;
      contact.Save();
      return contact;
    }

    private micros.DrxUzbekistan.IPerson UpdatePersonFromJSon (Newtonsoft.Json.Linq.JObject document, string route, micros.DrxUzbekistan.IPerson person)
    {
      if (person == null) person = micros.DrxUzbekistan.People.Create();
      var fullName = document.SelectToken($"{route}.{route}_name").ToString().ToUpper().Split(new char[0], StringSplitOptions.RemoveEmptyEntries);
      person.LastName = fullName[0];
      person.FirstName = fullName[1];
      person.MiddleName = fullName[2];
      if (JTokenIsNullOrEmpty(document.SelectToken($"{route}.{route}_account")))
        person.Account = document.SelectToken($"{route}.{route}_account").ToString();
      if (JTokenIsNullOrEmpty(document.SelectToken($"{route}.{route}_address")))
        person.LegalAddress = document.SelectToken($"{route}.{route}_address").ToString();
      person.Save();
      return person;
    }
    
    /// <summary>
    /// Экспорт подписи из справочника интеграции
    /// </summary>
    /// <param name="databook">Справочник интеграции</param>
    /// <returns>Архив с подписью</returns>
    [Public, Remote]
    public IZip ExportSign(IIntegrationDatabook databook)
    {
      var zip=Sungero.Core.Zip.Create();
      var data = Encoding.Default.GetBytes(Convert.ToBase64String(databook.Sign));
      zip.Add(data,"sign","txt");
      zip.Save("sign.zip");
      return zip;
    }
    
    /// <summary>
    /// Возвращает подпись, сделанную в Директуме, из справочника интеграции
    /// </summary>
    /// <param name="databook">справочник интеграции</param>
    /// <returns>Архив с подписью</returns>
    [Public, Remote]
    public IZip ExportAction(IIntegrationDatabook databook)
    {
      var zip=Sungero.Core.Zip.Create();
      var data = Encoding.Default.GetBytes(Convert.ToBase64String(databook.OutgoingData));
      zip.Add(data,"sign","txt");
      zip.Save("sign.zip");
      return zip;
    }
    
    [Public, Remote]
    public IZip ExportData(string exportData)
    {
      var zip = Sungero.Core.Zip.Create();
      var data = Encoding.Default.GetBytes(exportData);
      zip.Add(data, "data", "txt");
      zip.Save("data.zip");
      return zip;
    }
    
    /// <summary>
    /// Возвращает json документа
    /// </summary>
    /// <param name="databook">Справочник интеграции</param>
    /// <returns>Архив с Json</returns>
    [Public, Remote]
    public IZip ExportJson(IIntegrationDatabook databook)
    {
      var zip=Sungero.Core.Zip.Create();
      zip.Add(databook.JSon,"json","json");
      zip.Save("json.zip");
      return zip;
    }
    
    /// <summary>
    /// Проверка на наличие значения в токене
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    public static bool JTokenIsNullOrEmpty(Newtonsoft.Json.Linq.JToken token)
    {
      return (token == null) ||
        (token.Type == JTokenType.Array && !token.HasValues) ||
        (token.Type == JTokenType.Object && !token.HasValues) ||
        (token.Type == JTokenType.String && token.ToString() == String.Empty) ||
        (token.Type == JTokenType.Null);
    }
    
    #endregion
    
    #region Создание Исх. Эл. Договора
    [Public, Remote]
    public virtual string FillElectronicContract(micros.MultibankModule.IEContract contract)
    {
      var sContract = micros.MultibankModule.Structures.Module.ContractJson.Create();
      sContract.contract_name = contract.Title;
      sContract.place = contract.ContractAddress;
      sContract.expire_date = contract.ValidTill.Value.ToString("yyyy-MM-dd");
      
      List<Structures.Module.ITermJson> terms = new List<micros.MultibankModule.Structures.Module.ITermJson>();
      foreach (var t in contract.Terms)
      {
        var term = Structures.Module.TermJson.Create();
        term.ord_no = t.Number.Value;
        term.term_title = t.TermTitle;
        term.term_text = t.TermText;
        terms.Add(term);
      }
      
      List<Structures.Module.IGoodJson> goods = new List<micros.MultibankModule.Structures.Module.IGoodJson>();
      foreach (var g in contract.Goods)
      {
        var good = Structures.Module.GoodJson.Create();
        good.name = g.Name;
        good.catalog_code = g.CatalogCode;
        good.catalog_name = g.CatalogName;
        good.unit = g.Unit.MeasureId.Value;
        good.qty = g.Qty.Value;
        good.price = g.Price.Value;
        good.amount = g.Amount.Value;
        good.vat_percent = g.VatPercent.Value.ToString();
        good.vat_sum = g.VatSum.Value;
        good.vat_total_sum = g.VatTotalSum.Value;
        good.no = g.No.Value;
        good.total_sum = g.TotalSum.Value;
        good.without_vat = g.WithoutVat.Value;
        goods.Add(good);
      }
      
      var company = micros.DrxUzbekistan.BusinessUnits.As(contract.BusinessUnit);
      var owner = Structures.Module.OwnerJson.Create();
      if (!String.IsNullOrEmpty(company.TIN))owner.ow_tin = company.TIN;
      if (!String.IsNullOrEmpty(company.Name))owner.ow_name = company.Name;
      if (!String.IsNullOrEmpty(company.LegalAddress))owner.ow_address = company.LegalAddress;
      char[] split = new char[]{',',';'};
      if (!String.IsNullOrEmpty(company.Phones))owner.ow_work_phone = company.Phones.Split(split, StringSplitOptions.RemoveEmptyEntries)[0];
      owner.ow_mobile = null;
      owner.ow_fax = null;
      if (!String.IsNullOrEmpty(company.OKEDmicros))owner.ow_oked = company.OKEDmicros;
      if (!String.IsNullOrEmpty(company.Account))owner.ow_account = company.Account;
      if (company.Bank != null)
        if (!String.IsNullOrEmpty(company.Bank.BIC))owner.ow_bank_id = company.Bank.BIC;
      owner.ow_fiz_tin = Convert.ToInt64(micros.DrxUzbekistan.People.As(company.CEO.Person).PINImicros);
      owner.ow_fio = company.CEO.Person.Name;
      owner.ow_branch_code = null;
      owner.ow_branch_name = null;
      
      var contragent = micros.DrxUzbekistan.Companies.As(contract.Counterparty);
      List<Structures.Module.IParticipantsJson> participants = new List<micros.MultibankModule.Structures.Module.IParticipantsJson>();
      var participant = Structures.Module.ParticipantsJson.Create();
      if (!String.IsNullOrEmpty(contragent.TIN))participant.pt_tin = contragent.TIN;
      if (!String.IsNullOrEmpty(contragent.Name))participant.pt_name = contragent.Name;
      if (!String.IsNullOrEmpty(contragent.LegalAddress))participant.pt_address = contragent.LegalAddress;
      if (!String.IsNullOrEmpty(contragent.Phones))participant.pt_work_phone = contragent.Phones.Split(split, StringSplitOptions.RemoveEmptyEntries)[0];
      participant.pt_mobile = null;
      participant.pt_fax = null;
      if (!String.IsNullOrEmpty(contragent.OKEDmicros))participant.pt_oked = contragent.OKEDmicros;
      if (!String.IsNullOrEmpty(contragent.Account))participant.pt_account = contragent.Account;
      if (contragent.Bank != null)
        if (!String.IsNullOrEmpty(contragent.Bank.BIC))participant.pt_bank_id = contragent.Bank.BIC;
      if (!String.IsNullOrEmpty(micros.DrxUzbekistan.People.As(contract.CounterpartySignatory.Person).PINImicros))
      {
        var pinfl = micros.DrxUzbekistan.People.As(contract.CounterpartySignatory.Person).PINImicros;
        participant.pt_fiz_tin = Convert.ToInt64(pinfl);
      }
      participant.pt_fio = contract.CounterpartySignatory.Name;
      participant.pt_branch_code = null;
      participant.pt_branch_name = null;
      participants.Add(participant);
      
      var eContract = micros.MultibankModule.Structures.Module.EContractJson.Create();
      eContract.contract = sContract;
      eContract.сontract_name = sContract.contract_name;
      eContract.expire_date = sContract.expire_date;
      eContract.place = sContract.place;
      eContract.document_number = contract.RegistrationNumber;
      eContract.document_date = contract.ValidFrom.Value.ToString("yyyy-MM-dd");
      eContract.has_vat = contract.Goods.Any(x => x.WithoutVat == false);
      eContract.owner = owner;
      eContract.participants = participants;
      eContract.terms = terms;
      eContract.goods = goods;
      
      string json = JsonConvert.SerializeObject(eContract);
      return json;
    }
    
    [Public, Remote]
    public virtual string EContractErrors(micros.MultibankModule.IEContract contract)
    {
      List<string> errorsList = new List<string>();
      string errors = String.Empty;
      
      if (String.IsNullOrEmpty(contract.RegistrationNumber)) errorsList.Add("Зарегистрируйте документ");
      
      //Наша орг
      if (String.IsNullOrEmpty(contract.BusinessUnit.TIN)) errorsList.Add("Заполните ИНН нашей организации");
      if (contract.BusinessUnit.CEO == null) errorsList.Add("В нашей организации отсутствует руководитель");
      if (contract.BusinessUnit.CEO != null)
        if (String.IsNullOrEmpty(micros.DrxUzbekistan.People.As(contract.BusinessUnit.CEO.Person).PINImicros)) errorsList.Add("Заполните ИНН руководителя нашей организации");
      if (String.IsNullOrEmpty(contract.BusinessUnit.Phones)) errorsList.Add("Заполните номер нашей организации");
      if (String.IsNullOrEmpty(micros.DrxUzbekistan.BusinessUnits.As(contract.BusinessUnit).OKEDmicros)) errorsList.Add("Заполните ОКЭД нашей организации");
      if (String.IsNullOrEmpty(contract.BusinessUnit.Account)) errorsList.Add("Заполните номер счета нашей организации");
      if (String.IsNullOrEmpty(contract.BusinessUnit.LegalAddress)) errorsList.Add("Заполните адресс нашей организации");
      if (contract.BusinessUnit.Bank == null) errorsList.Add("Заполните банк в нашей организации");
      if (contract.BusinessUnit.Bank != null)
        if (String.IsNullOrEmpty(contract.BusinessUnit.Bank.BIC)) errorsList.Add("Заполните МФО в банке нашей организации");
      
      //Контакт
      if (contract.CounterpartySignatory.Person == null) errorsList.Add("У директора отсутствует персона");
      if (contract.CounterpartySignatory.Person != null && String.IsNullOrEmpty(micros.DrxUzbekistan.People.As(contract.CounterpartySignatory.Person).PINImicros)) errorsList.Add("Заполните ПИНФЛ директора в персоне");
      
      //Контрагент
      if (String.IsNullOrEmpty(contract.Counterparty.TIN)) errorsList.Add("Заполните ИНН контрагента");
      if (String.IsNullOrEmpty(contract.Counterparty.Account)) errorsList.Add("Заполните номер счета контрагента");
      if (String.IsNullOrEmpty(contract.Counterparty.LegalAddress)) errorsList.Add("Заполните адресс контрагента");
      if (contract.Counterparty.Bank == null) errorsList.Add("Заполните банк в контрагенте");
      if (contract.Counterparty.Bank != null)
        if (String.IsNullOrEmpty(contract.Counterparty.Bank.BIC)) errorsList.Add("Заполните МФО в банке контрагента");
      if (String.IsNullOrEmpty(micros.DrxUzbekistan.Companies.As(contract.Counterparty).OKEDmicros)) errorsList.Add("Заполните ОКЭД контрагента");
      
      //Условия договора и товары
      if (contract.Terms.Count < 1) errorsList.Add("Должно быть хотя бы одно условие договора");
      if (contract.Goods.Count < 1) errorsList.Add("Должен быть хотя бы один продукт или услуга");
      
      
      foreach (string error in errorsList)
      {
        errors = errors + "\r\n" + error;
      }
      return errors;
    }
    
    [Public, Remote]
    public virtual IIntegrationDatabook CreateDatabook(Sungero.Docflow.IOfficialDocument document, string json, string response)
    {
      var databook = IntegrationDatabooks.Create();
      databook.Document = document;
      databook.Name = document.Name;
      databook.JSon = Encoding.Default.GetBytes(json);
      databook.IsIncoming = false;
      databook.Guid = JObject.Parse(response).SelectToken("data.document_id").ToString();
      databook.Save();
      return databook;
    }
    #endregion
  }
}