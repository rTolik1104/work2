﻿using System;
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
    //    [Public]
    //    public string RefreshToken(string refreshToken)
    //    {
    //      var company = Sungero.Company.BusinessUnits.GetAll().Where(x => x.TIN = json.SelectToken("document.document_data.sellertin").ToString()).FirstOrDefault();
    //      var box = this.GetIntegrationDataBookForCompany(company);
    //      var client = new RestClient(box.ExchangeService.GetUpdatedmicros);
    //      client.Timeout = -1;
    //      var request = new RestRequest(Method.POST);
    //      request.AlwaysMultipartFormData = true;
    //      request.AddParameter("refresh_token", refreshToken);
    //      request.AddParameter("client_id", box.ClientIdmicros);
    //      request.AddParameter("client_secret", box.ClientSecretmicros);
    //      request.AddParameter("scope", "*");
    //      IRestResponse response = client.Execute(request);
    //      return response.Content;
    //    }
    
    /// <summary>
    /// Метод получения списка доступных для авторизации профилей юридических и физических лиц
    /// </summary>
    /// <param name="token">Токен доступа</param>
    /// <returns></returns>
    [Public]
    public string GetProfile(micros.MultibankSolution.IBusinessUnitBox box)
    {
      var client = new RestClient(box.ExchangeService.GetProfilemicros);
      client.Authenticator = new JwtAuthenticator(Encoding.Default.GetString(box.AccessTokenmicros));
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
    public string JoinProfile(string token, string profileId, Sungero.Company.IBusinessUnit company)
    {
      var box = this.GetBuisnesUnitBoxForCompany(company);
      var client = new RestClient(box.ExchangeService.JoinProfilemicros);
      client.Authenticator = new JwtAuthenticator(token);
      var request = new RestRequest(Method.POST);
      request.AlwaysMultipartFormData = true;
      request.AddParameter("profile_id", profileId);
      IRestResponse response = client.Execute(request);
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
      //JArray jArray = JArray.Parse(Base64DecodeObject(parameter).ToString());
      var json = JObject.Parse(JsonConvert.DeserializeObject<JObject>(Base64DecodeObject(parameter)).ToString().ToLower());
      
      // Проверка ИНН
      //if(json.SelectToken("document.document_data.sellertin").ToString() == "" || json.SelectToken("document.document_data.buyertin").ToString() == "")
      //  return "Инн покупателя или продавца не заполнена";
      
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
      else if(doctype == Constants.Module.DoctypeId.PowerOfAttorney)
        return "Неверный тип документа";
      else if(doctype == Constants.Module.DoctypeId.TTH)
        return "Неверный тип документа";
      
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
        //this.UpdateTaxInvoice(json.ToString(), databook);
        //var task = ReviewTasks.GetAll().Where(x => x.IntegrationDatabook == databook).FirstOrDefault();
        //if (task != null) this.UpdateDocumentStatusForMBKAssignment(task);
        return "OK - " + databook.Document.Info.ToString();
      }
    }
    
    /// <summary>
    /// Обновить СФ из Json
    /// </summary>
    /// <param name="jsonString"></param>
    /// <param name="documentOld"></param>
    /// <param name="databook"></param>
    [Public, Remote]
    public virtual void UpdateTaxInvoice(string jsonString, IIntegrationDatabook databook)
    {
      var document = Sungero.Docflow.AccountingDocumentBases.As(databook.Document);
      JObject json = JObject.Parse(jsonString.ToLower());
      JToken goods = json.SelectToken("document.goods");
      string buyerTin = json.SelectToken("document.document_data.buyertin").ToString();
      string sellerTin = json.SelectToken("document.document_data.sellertin").ToString();
      string ourTin = String.Empty;
      string counterTin = String.Empty;
      
      if (databook.IsIncoming.Value)
      {
        ourTin = buyerTin;
        counterTin = sellerTin;
      }
      else
      {
        ourTin = sellerTin;
        counterTin = buyerTin;
      }
      
      document.Currency = Sungero.Commons.Currencies.GetAll().Where(x => x.AlphaCode == "UZS").FirstOrDefault();
      double amount = 0;
      if(json.SelectToken("document.goods.[0].total_sum") != null)
      {
        foreach(var summ in json.SelectToken("document.goods").ToArray())
        {
          amount = amount + Convert.ToDouble(summ.SelectToken("total_sum"));
        }
      }
      document.TotalAmount = amount;
      document.BusinessUnit = Sungero.Company.BusinessUnits.GetAll().Where(x => x.TIN == ourTin).FirstOrDefault();
      document.Counterparty = Sungero.Parties.Companies.GetAll().Where(x => x.TIN == counterTin).FirstOrDefault();
      document.RegistrationNumber = json.SelectToken("document.document_number").ToString();
      document.RegistrationDate = Convert.ToDateTime(json.SelectToken("document.document_date").ToString());
      //  Если свойства Наша организация или Контрагент пустая, создать новую на основе данных из json
      
      #region Buyer params
      string bName = json.SelectToken("document.document_data.buyer.name").ToString();
      string bAddress = json.SelectToken("document.document_data.buyer.address").ToString();
      string bAccount = json.SelectToken("document.document_data.buyer.account").ToString();
      string bVAT = json.SelectToken("document.document_data.buyer.vatregcode").ToString();
      string bBank = json.SelectToken("document.document_data.buyer.bankid").ToString();
      #endregion

      #region Seller params
      string sName = json.SelectToken("document.document_data.seller.name").ToString();
      string sAddress = json.SelectToken("document.document_data.seller.address").ToString();
      string sAccount = json.SelectToken("document.document_data.seller.account").ToString();
      string sVAT = json.SelectToken("document.document_data.seller.vatregcode").ToString();
      string sBank = json.SelectToken("document.document_data.seller.bankid").ToString();
      #endregion
      
      if(document.Counterparty == null)
      {
        if (databook.IsIncoming.Value) document.Counterparty = Create_Counterparty(sName.ToUpper(), sellerTin, sAddress, sAccount, sVAT, sBank);
        else document.Counterparty = Create_Counterparty(bName.ToUpper(), buyerTin, bAddress, bAccount, bVAT, bBank);
      }

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
      var company = Sungero.Company.BusinessUnits.GetAll().Where(x => x.TIN == companyTin).FirstOrDefault();
      var box = this.GetBuisnesUnitBoxForCompany(company);
      this.IfNeedRefreshTokens(box);
      if(!IntegrationDatabooks.GetAll().Any(x => x.Guid == json.SelectToken("document.document_id").ToString() && x.IsIncoming == isIncoming))
      {
        var databook = IntegrationDatabooks.Create();
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
        var databook = IntegrationDatabooks.GetAll().Where(x => x.Guid == json.SelectToken("document.document_id").ToString() && x.IsIncoming == isIncoming).FirstOrDefault();
        databook.JSon = Encoding.Default.GetBytes(json.ToString());
        databook.Save();
        //this.UpdateContractStatementFromJson(json.ToString(), databook.Document, isIncoming);
        //var task = ReviewTasks.GetAll().Where(x => x.IntegrationDatabook == databook).FirstOrDefault();
        //if (task != null) this.UpdateDocumentStatusForMBKAssignment(task);
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
      double amount = 0;
      if(json.SelectToken("document.goods.[0].amount") != null)
      {
        foreach(var summ in json.SelectToken("document.goods").ToArray())
        {
          amount = amount + Convert.ToDouble(summ.SelectToken("amount"));
        }
      }
      document.TotalAmount = amount;
      if (isIncoming == true)
      {
        
        document.BusinessUnit = Sungero.Company.BusinessUnits.GetAll().Where(x => x.TIN == clientTin).FirstOrDefault();
        document.Counterparty = Sungero.Parties.Counterparties.GetAll().Where(x => x.TIN == ownerTin).FirstOrDefault();
      }
      else
      {
        document.BusinessUnit = Sungero.Company.BusinessUnits.GetAll().Where(x => x.TIN == ownerTin).FirstOrDefault();
        document.Counterparty = Sungero.Parties.Counterparties.GetAll().Where(x => x.TIN == clientTin).FirstOrDefault();
        
      }
      
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
        var document = Sungero.Contracts.Contracts.Create();
        document.Save();
        databook.Document = document;
        databook.Save();
        this.UpdateContractFromJson(json.ToString(), document, isIncoming);

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
        //this.UpdateContractFromJson(json.ToString(), databook.Document, isIncoming);
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
    public virtual void UpdateContractFromJson(string jsonString, Sungero.Docflow.IOfficialDocument documentOLD, bool isIncoming)
    {
      var document = MultibankSolution.Contracts.Get(documentOLD.Id);
      if (Locks.GetLockInfo(document).IsLocked) Locks.Unlock(document);
      JObject json = JObject.Parse(jsonString);
      string ownerTin = json.SelectToken("owner.owner_tin").ToString();
      string contragentTin = json.SelectToken("contragent.contragent_tin").ToString();
      
      document.Currency = Sungero.Commons.Currencies.GetAll().Where(x => x.AlphaCode == "UZS").FirstOrDefault();
      double amount = 0;
      if(json.SelectToken("document.goods.[0].total_sum") != null)
      {
        foreach(var summ in json.SelectToken("document.goods").ToArray())
        {
          amount = amount + Convert.ToDouble(summ.SelectToken("total_sum"));
        }
      }
      document.TotalAmount = amount;
      if (isIncoming == true)
      {
        document.BusinessUnit = Sungero.Company.BusinessUnits.GetAll().Where(x => x.TIN == contragentTin).FirstOrDefault();
        document.Counterparty = Sungero.Parties.Companies.GetAll().Where(x => x.TIN == ownerTin).FirstOrDefault();
        
        if(document.BusinessUnit == null)
        {
          #region Buyer params
          string bName = json.SelectToken("conteragent.document_contragent_name").ToString();
          #endregion
          
          document.BusinessUnit = Create_BuisnessUnit(bName.ToUpper(), contragentTin, null, null, null, null);
        }
        if(document.Counterparty == null)
        {
          #region Seller params
          string sName = json.SelectToken("owner.document_owner_name").ToString();
          #endregion
          
          document.Counterparty = Create_Counterparty(sName.ToUpper(), ownerTin, null, null, null, null);
        }
      }
      else
      {
        document.BusinessUnit = Sungero.Company.BusinessUnits.GetAll().Where(x => x.TIN == ownerTin).FirstOrDefault();
        document.Counterparty = Sungero.Parties.Companies.GetAll().Where(x => x.TIN == contragentTin).FirstOrDefault();
        
        if(document.BusinessUnit == null)
        {
          #region Seller params
          string sName = json.SelectToken("owner.document_owner_name").ToString();
          #endregion
          
          
          document.BusinessUnit = Create_BuisnessUnit(sName.ToUpper(), ownerTin, null, null, null, null);
        }
        if(document.Counterparty == null)
        {
          #region Buyer params
          string bName = json.SelectToken("conteragent.document_contragent_name").ToString();
          #endregion
          
          document.Counterparty = Create_Counterparty(bName.ToUpper(), contragentTin, null, null, null, null);
        }
      }
      
      document.RegistrationNumber = json.SelectToken("document.document_number").ToString();
      if (document.Subject == null) document.Subject = json.SelectToken("document.document_number").ToString();
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
        Logger.Debug("BodyOLD: " + Encoding.Default.GetString(bodyOld));
        Logger.Debug("NewBody: " + Encoding.Default.GetString(body));
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
      task.Subject = "Получен документ : " + document.Name;
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
      Logger.Debug("Test_pcs7: " + forSign);
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
      string document_id = IntegrationDatabooks.GetAll().Where(x => x.Document == document).FirstOrDefault().Guid;
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
      string document_id = IntegrationDatabooks.GetAll().Where(x => x.Document == document).FirstOrDefault().Guid;
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
      //var databook = IntegrationDatabooks.GetAll().Where(x => x.Document == document).FirstOrDefault();
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

      Logger.Debug("pkcs7: " + signature);
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
      Logger.Debug("PKCS7: " + externalSign);
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
            
            if (!Signatures.Get(document.LastVersion).Any(x => x.SignatoryFullName == name))
            {
              Signatures.Import(document, SignatureType.Approval, name, sgn, document.LastVersion);
            }
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
      Logger.Debug("DATA: " + signedData.SignerInfos.ToArray().LastOrDefault().ToString());
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
      string document_id = IntegrationDatabooks.GetAll().Where(x => x.Document == document).FirstOrDefault().Guid;
      var client = new RestClient(this.GetBuisnesUnitBoxForCompany(document.BusinessUnit).ExchangeService.GetPublicDocmicros + document_id);
      client.Authenticator = new JwtAuthenticator(this.GetMultibankToken(document.BusinessUnit));
      var request = new RestRequest(Method.GET);
      request.AddParameter("params","public");
      IRestResponse response = client.Execute(request);
      Logger.Debug("Update data: " + response.Content);
      JObject json = JObject.Parse(response.Content);
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
        //if(box.MultibankCompanymicros != null)
        //MultibankModule.PublicFunctions.Module.JoinProfile(Encoding.Default.GetString(box.AccessTokenmicros), box.MultibankCompanymicros.ProfileID);
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
    #endregion
  }
}