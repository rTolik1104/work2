using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using RestSharp.Authenticators;
using micros.Multibank;
using Sungero.Domain.Shared;
using Sungero.Metadata;


namespace micros.Multibank.Server
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
    private string Base64DecodeObject(string base64String)
    {
      var base64EncodedBytes = Convert.FromBase64String(base64String);
      return JsonConvert.DeserializeObject<string>(System.Text.Encoding.UTF8.GetString(base64EncodedBytes));
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
          new JProperty("data",
                        new JObject(
                          new JProperty("invoice_type", json.SelectToken("document.invoice_type").ToString()),
                          new JProperty("is_one_sided_invoice", json.SelectToken("document.is_one_sided_invoice").ToString()),
                          new JProperty("one_sided_invoice_type", json.SelectToken("document.one_sided_invoice_type").ToString()),
                          new JProperty("invoice",
                                        new JObject(
                                          new JProperty("document_numer", json.SelectToken("document.invoice.document_date").ToString()),
                                          new JProperty("document_date", json.SelectToken("document.invoice.document_number").ToString()))),
                          new JProperty("contract",
                                        new JObject(
                                          new JProperty("contract_no", json.SelectToken("document.contract.document_contract_date").ToString()),
                                          new JProperty("contract_date", json.SelectToken("document.contract.document_contract_no").ToString()))),
                          new JProperty("old_invoice_doc",
                                        new JObject(
                                          new JProperty("old_invoice_id", json.SelectToken("document.old_invoice_doc.old_invoice_id").ToString()),
                                          new JProperty("old_invoice_number", json.SelectToken("document.old_invoice_doc.old_invoice_number").ToString()),
                                          new JProperty("old_invoice_date", json.SelectToken("document.old_invoice_doc.old_invoice_date").ToString()))),
                          new JProperty("power_of_attorney",
                                        new JObject(
                                          new JProperty("attorney_no", json.SelectToken("document.power_of_attorney.attorney_no").ToString()),
                                          new JProperty("attorney_date_of_issue", json.SelectToken("document.power_of_attorney.attorney_date_of_issue").ToString()),
                                          new JProperty("attroney_confidant_tin", json.SelectToken("document.power_of_attorney.attorney_confidant_tin").ToString()),
                                          new JProperty("attroney_confidant_person_id", json.SelectToken("document.power_of_attorney.attorney_confidant_person_id").ToString()),
                                          new JProperty("attorney_confidant_fio", json.SelectToken("document.power_of_attorney.attorney_confidant_fio").ToString()))),
                          new JProperty("storekeeper",
                                        new JObject(
                                          new JProperty("storekeeper_tin", json.SelectToken("document.storekeeper.storekeeper_tin").ToString()),
                                          new JProperty("storekeeper_full_name", json.SelectToken("document.storekeeper.owner_released_the_goods_name").ToString()),
                                          new JProperty("storekeeper_person_id", json.SelectToken("document.storekeeper.storekeeper_person_id").ToString()))),
                          new JProperty("contragent_tin", json.SelectToken("document.contragent_tin").ToString()),
                          new JProperty("foreign_counterparty",
                                        new JObject(
                                          new JProperty("country_id", json.SelectToken("document.foreign_counterparty.foreign_country_id").ToString()),
                                          new JProperty("foreign_counterparty_name", json.SelectToken("document.foreign_counterparty.foreign_counterparty_name").ToString()),
                                          new JProperty("foreign_counterparty_address", json.SelectToken("document.foreign_counterparty.foreign_counterparty_address").ToString()),
                                          new JProperty("foreign_counterparty_bank", json.SelectToken("document.foreign_counterparty.foreign_counterparty_bank").ToString()),
                                          new JProperty("foreign_counterparty_account", json.SelectToken("document.foreign_counterparty.foreign_counterparty_account").ToString()))),
                          new JProperty("goods", json.SelectToken("document.goods")))));
      }
      catch
      {
        return "null";  // Приходится возвращать текст null потому-что это не C# 8.0 -_-
      }
      
      
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
      var client = new RestClient("https://api-staging.multibank.uz/api/v1/get_viewer_document");
      var request = new RestRequest(Method.POST);
      request.AddParameter("is_new", "false");
      request.AddParameter("document_id", json.SelectToken("document.document_id").ToString());
      request.AddParameter("password", json.SelectToken("document.document_password_owner").ToString());
      IRestResponse response = client.Execute(request);
      return response.IsSuccessful ? JObject.Parse(response.Content).SelectToken("data").ToString() : JObject.Parse(response.Content).SelectToken("success").ToString();
    }
    
    /// <summary>
    /// Создание ЭСФ версии 2.0 в Мультибанке.
    /// </summary>
    /// <param name="jsonString">Json для отправки(ЭСФ 2.0).</param>
    /// <returns>Ответ в виде string</returns>
    [Public]
    public string SendDocument(string jsonString)
    {
      JObject json = JObject.Parse(jsonString);
      
      var client = new RestClient("https://api-staging.multibank.uz/api/v1/document");
      client.Authenticator = new HttpBasicAuthenticator("", "");
      var request = new RestRequest(Method.POST);
      request.AddParameter("document_id", Constants.Module.DoctypeId.Invoice);
      request.AddParameter("data", json);
      IRestResponse response = client.Execute(request);
      return response.Content;
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
      JObject json = JObject.Parse(Base64DecodeObject(parameter));
      
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
    
    /// <summary>
    /// Создание документа "Счет-фактура полученный" - (IncomingTaxInvoice)
    /// </summary>
    /// <param name="json">Json по которому будет создан документ</param>
    /// <returns>Ответ об успехе</returns>
    public virtual string Create_IncomingInvoice(Newtonsoft.Json.Linq.JObject json)
    {
      var document = micros.MultibankSolution.IncomingTaxInvoices.Create();
      
      string route = "document.document_data.";
      string buyerTin = json.SelectToken(route + "BuyerTin").ToString();
      string sellerTin = json.SelectToken(route + "SellerTin").ToString();
      
      document.Guidmicros = json.SelectToken("document.document_id").ToString();
      document.Currency = Sungero.Commons.Currencies.GetAll().Where(x => x.AlphaCode == "UZS").FirstOrDefault();
      if(json.SelectToken("document.goods.[0].total_sum") != null)
        document.TotalAmount = Convert.ToDouble(json.SelectToken("document.goods.[0].total_sum"));
      document.BusinessUnit = Sungero.Company.BusinessUnits.GetAll().Where(x => x.TIN == buyerTin).FirstOrDefault();
      document.Counterparty = Sungero.Parties.Companies.GetAll().Where(x => x.TIN == sellerTin).FirstOrDefault();
      //  Если свойства Наша организация или Контрагент пустая, создать новую на основе данных из json
      if(document.BusinessUnit == null) document.BusinessUnit = Create_BuisnessUnit(json.SelectToken(route + "Buyer.Name").ToString(), buyerTin, json.SelectToken(route + "Buyer.Address").ToString(), json.SelectToken(route + "Buyer.Account").ToString(), json.SelectToken(route + "Buyer.VatRegCode").ToString(), json.SelectToken(route + "Buyer.BankId").ToString());
      if(document.Counterparty == null) document.Counterparty = Create_Counterparty(json.SelectToken(route + "Seller.Name").ToString(), sellerTin, json.SelectToken(route + "Seller.Address").ToString(), json.SelectToken(route + "Seller.Account").ToString(), json.SelectToken(route + "Seller.VatRegCode").ToString(), json.SelectToken(route + "Seller.BankId").ToString());

      document.CreateVersion();
      var  version = document.LastVersion;
      
      byte[] jsonByte = Convert.FromBase64String(Base64EncodeObject(json));
      MemoryStream jsonStream = new MemoryStream(jsonByte);
      version.Body.Write(jsonStream);
      version.BodyAssociatedApplication = Sungero.Content.AssociatedApplications.GetByExtension("json");
      
      string pdfBase64 = GetPDF(json);
      if(pdfBase64 != "false" && pdfBase64 != "False" && pdfBase64 != null)
      {
        byte[] pdfByte = Convert.FromBase64String(pdfBase64);
        MemoryStream pdfStream = new MemoryStream(pdfByte);
        version.PublicBody.Write(pdfStream);
        version.AssociatedApplication = Sungero.Content.AssociatedApplications.GetByExtension("pdf");
      }
      else
        version.AssociatedApplication = Sungero.Content.AssociatedApplications.GetByExtension("json");
      
      document.Save();
      
      return "OK - " + document.Info.ToString();
    }
    #endregion
    
    #region Создание записей в справочнике
    
    /// <summary>
    /// Создание сущности "Наша орг.",
    /// </summary>
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
    
    /// <summary>
    /// Создание сущности "Организация"
    /// </summary>
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
    [Public, Remote]
    public void MultibankSign(Sungero.Content.IElectronicDocument document)
    {
      //string json = Encoding.UTF8.(StreamReader.document.Versions.FirstOrDefault().Body);
      JObject json;
      using (var sr = new StreamReader(document.Versions.FirstOrDefault().Body.Read()))
        using (var jsonTextReader = new JsonTextReader(sr))
          json = JObject.Parse(new JsonSerializer().Deserialize(jsonTextReader).ToString());
      
      
      string document_id = json.SelectToken("document.document_id").ToString();
      
      string gnkString = this.GNKString(document, document_id);
      //string pkcs7String = PublicFunctions.Module.SendToEimzo(gnkString);
      //this.SignMultibankDocument(pkcs7String, document_id);

    }
    [Remote]
    public string GNKString(Sungero.Content.IElectronicDocument document, string document_id)
    {
      
      var client = new RestClient("https://api-staging.multibank.uz/api/v1/document/data_for_signing");
      client.Authenticator = new JwtAuthenticator("eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiIsImp0aSI6IjdjZGVmNjMzM2U4MTgyZWUyMmYxMDIxNjU5NTVhZTc3YmNhMTI3MzkxMGNmZjQ1YmJlOWY5MDQ0M2IyOWU4OTY4OThhNTdjNzJjMmIyYzNkIn0.eyJhdWQiOiIyIiwianRpIjoiN2NkZWY2MzMzZTgxODJlZTIyZjEwMjE2NTk1NWFlNzdiY2ExMjczOTEwY2ZmNDViYmU5ZjkwNDQzYjI5ZTg5Njg5OGE1N2M3MmMyYjJjM2QiLCJpYXQiOjE2MzkxMTY3NzcsIm5iZiI6MTYzOTExNjc3NywiZXhwIjoxNjM5Mjg5NTc3LCJzdWIiOiI5ODMwYWNlZC05ZjEzLTRkYjktODJmNC1jZDY5YjYxMzljYTAiLCJzY29wZXMiOlsiY29tcGFueSIsInVzZXIiLCJwcm9maWxlIiwicGVyc29uIl19.SroYA1a9zy3Y4A2e1_1QwVbjkOD_01v_UBnDUPVjHfRtFdjMVrQC_pPgbtctSOszIEqgFMbZrhiOQbKVQ3K8i2kE2tq5wsVqhhitnlZ1HO6abtG0r_-lzHEOujUl7Ww7VasFjnqJocysCLCJH5u3XhIUK9LhOZzpKAvkvIIciaIpCDeU27hVk1h42iTipfbbVLvEROvu7I_RbWCctadcjtt_wKlWGicWtymg2fVE_y6Mi2zhZzC0mEqvJOchGLSyuu7Ijzkvt7k-gZFKzCgJKrXcq6TydbevCbMKH3C7-OND2BXIJgrUO6yu-tiaVkbHpZfa1jJH3F66ypBKLTCkEJau7B27sv3jHRFrn_8gQoVpB6leQLYiniuWOjPtAsAvkcFRVv7xIwWaCqngN6C_UViAONBBU8B6xZhwmOaxhxc-vdbMk-j0XQXJjKlj8_QUTyd9qdyOG6anIGZbTCmOLPy1jva-CIHtT2HYWzfw3wbWxc48x-E32oNRQbuc8PcwWShnMsEDFWtanHG9sBn6tbEVWP1jdkvMhIKNIyJ1eJ6n2KVl-ygepjOHUW0inTJnGo7HvcOv0n8J7zrgg686KFjfynpYNvxK4OyR4guU0gHQj-Pvtxw6Fs6D_TnvJRQtol8BW_tdlFhXMODSu_zj8ClGMouoXmuQMsaTRUOz1tQ");
      var request = new RestRequest(Method.GET);
      request.AddParameter("document_id", document_id);
      request.AddParameter("action", "signing");
      IRestResponse response = client.Execute(request);
      return response.Content.ToString();
    }
    
    public void SignMultibankDocument(string pkcs7String, string document_id)
    {
      var client = new RestClient("https://api-staging.multibank.uz/api/v1/signing");
      var request = new RestRequest(Method.POST);
      request.AddParameter("document_id", document_id);
      request.AddParameter("pkcs7", pkcs7String);
      IRestResponse response = client.Execute(request);
    }
    
    
    #region Create PDF
    [Public, Remote]
    public void CreatePDF(Sungero.Content.IElectronicDocument document)
    {
      var converter = Sungero.AsposeExtensions.Converter.Create();
      string html = "";
      if (document.GetEntityMetadata().GetOriginal().NameGuid.ToString() == "74c9ddd4-4bc4-42b6-8bb0-c91d5e21fb8a")
        html = micros.Multibank.Resources.IncomingTaxInvoiceHTML;
      byte[] bytes = Encoding.UTF8.GetBytes(html);
      using (MemoryStream stringStream = new MemoryStream(bytes))
      {
        var htmlToPDF = converter.ConvertHtmlToPdf(stringStream);
        
        if (document.LastVersion == null)
        {
          document.Versions.AddNew();
          this.WriteVersion(document, htmlToPDF);
        }
        else
          this.WriteVersion(document, htmlToPDF);
      }
    }
    public void WriteVersion(Sungero.Content.IElectronicDocument document, System.IO.MemoryStream htmlToPDF)
    {
      var version = document.LastVersion;
      version.PublicBody.Write(htmlToPDF);
      version.AssociatedApplication = Sungero.Content.AssociatedApplications.GetByExtension("pdf");

      document.Save();
      //version.Save();
    }
    public virtual string CreateGoodsTable(Sungero.Content.IElectronicDocument document)
    {
      string d = document.Versions.FirstOrDefault().Body.ToString();
      var json = Newtonsoft.Json.Linq.JObject.Parse(d);
      var allGoods = json.SelectToken("data.document.goods").ToList();
      string allgoodstable = " ";
      foreach (var good in allGoods)
      {
        string goodstring = micros.Multibank.Resources.GoodTableHTML;
        goodstring.Replace("{Good name}",good.SelectToken("Name").ToString());
        goodstring.Replace("{IKPU}",good.SelectToken("Name").ToString());
        goodstring.Replace("{Units",good.SelectToken("unit").ToString());
        goodstring.Replace("{Quantity}",good.SelectToken("unit").ToString());
        goodstring.Replace("{Cost}",good.SelectToken("price").ToString());
        goodstring.Replace("{Price}",good.SelectToken("amount").ToString());
        goodstring.Replace("{Bid}",good.SelectToken("Name").ToString());
        goodstring.Replace("{Summ}",good.SelectToken("Summa").ToString());
        goodstring.Replace("{NDSCost}",good.SelectToken("DeliverySumWithVat").ToString());
        allgoodstable = allgoodstable + goodstring;
      }
      return allgoodstable;
    }
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
      string json = this.GetJsonFromBody(document);
      string document_password_owner = this.GetJsonMultibankPassword(json);
      string document_id = this.GetJsonMultibankGuid(json);
      Logger.Debug("document.document_id " + document_id);
      var client = new RestClient("https://multibank.uz/api/public/get_public_document?");
      client.Authenticator = new HttpBasicAuthenticator("micros24site", "a8lo23d0r3f48");
      var request = new RestRequest(Method.GET);
      request.AddParameter("document_id", document_id);
      request.AddParameter("password", document_password_owner);
      IRestResponse response = client.Execute(request);
      JObject jsonIn = JObject.Parse(response.Content.ToString());
      if (jsonIn.SelectToken("data.signature_information.contragent") == null)
        return false;
      else
      {
        return true;
      }
      //return response.Content;
      
    }
    
    /// <summary>
    /// Импорт дописи документа полученного из Multibank
    /// </summary>
    /// <param name="signature_information">Токен из JSon мультибанка(data.signature_information)</param>
    /// <param name="document"></param>
    public virtual void RegistrSigning(Newtonsoft.Json.Linq.JObject signature_information, Sungero.Content.IElectronicDocument document)
    {
      if (signature_information.SelectToken("contragent") != null && document.HasVersions)
      {
        string signatoryFullName = "";
        string signData = signature_information.SelectToken("contragent").ToString();
        string tin = signData.Substring(signData.IndexOf("TIN") + 4, 9);
        Signatures.Import(document, SignatureType.Approval, signatoryFullName, Encoding.ASCII.GetBytes(signData), document.LastVersion);
      }
    }
    
    /// <summary>
    /// Получить из НЕ публичного тела документа json, если документ был загружен в виде json
    /// </summary>
    /// <param name="document">Документ</param>
    /// <returns>string json</returns>
    [Public]
    public virtual string GetJsonFromBody(Sungero.Content.IElectronicDocument document)
    {
      string json = "";
      //var version = document.LastVersion;
      //var sr = new StreamReader(version.Body.Read());
      //var jsonTextReader = new JsonTextReader(sr);
      using (var sr = new StreamReader(document.LastVersion.Body.Read()))
        using (var jsonTextReader = new JsonTextReader(sr))
          json = new JsonSerializer().Deserialize(jsonTextReader).ToString();
      //string json = new StreamReader(version.Body.Read(), Encoding.Unicode).ReadToEnd();
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
    #endregion
  }
}