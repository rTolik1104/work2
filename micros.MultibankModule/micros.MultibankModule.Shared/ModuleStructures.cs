using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace micros.MultibankModule.Structures.Module
{
  [Public]
  partial class EContractJson
  {
    public string сontract_name {get; set;}
    public string expire_date {get; set;}
    public string place {get; set;}
    public string document_number {get; set;}
    public string document_date {get; set;}
    public bool has_vat {get; set;}
    public micros.MultibankModule.Structures.Module.IContractJson contract {get; set;}
    public micros.MultibankModule.Structures.Module.IOwnerJson owner {get; set;}
    public List<micros.MultibankModule.Structures.Module.IParticipantsJson> participants {get; set;}
    public List<micros.MultibankModule.Structures.Module.ITermJson> terms {get; set;}
    public List<micros.MultibankModule.Structures.Module.IGoodJson> goods {get; set;}
  }
  
  [Public]
  partial class ContractJson
  {
    public string contract_name {get; set;}
    public string place {get; set;}
    public string expire_date {get; set;}
  }
  
  [Public]
  partial class OwnerJson
  {
    public string ow_tin {get; set;}
    public string ow_name {get; set;}
    public string ow_address {get; set;}
    public string ow_work_phone {get; set;}
    public string ow_mobile {get; set;}
    public string ow_fax {get; set;}
    public string ow_oked {get; set;}
    public string ow_account {get; set;}
    public string ow_bank_id {get; set;}
    public long ow_fiz_tin {get; set;}
    public string ow_fio {get; set;}
    public string ow_branch_code {get; set;}
    public string ow_branch_name {get; set;}
  }
  
  [Public]
  partial class ParticipantsJson
  {
    public string pt_tin {get; set;}
    public string pt_name {get; set;}
    public string pt_address {get; set;}
    public string pt_work_phone {get; set;}
    public string pt_mobile {get; set;}
    public string pt_fax {get; set;}
    public string pt_oked {get; set;}
    public string pt_account {get; set;}
    public string pt_bank_id {get; set;}
    public long pt_fiz_tin {get; set;}
    public string pt_fio {get; set;}
    public string pt_branch_code {get; set;}
    public string pt_branch_name {get; set;}
  }
  
  [Public]
  partial class TermJson
  {
    public int ord_no {get; set;}
    public string term_title {get; set;}
    public string term_text {get; set;}
  }
  
  [Public]
  partial class GoodJson
  {
    public string good_series {get; set;}
    public string name {get; set;}
    public string catalog_code {get; set;}
    public string catalog_name {get; set;}
    public int unit {get; set;}
    public int qty {get; set;}
    public double price {get; set;}
    public double amount {get; set;}
    public string vat_percent {get; set;}
    public double vat_sum {get; set;}
    public double vat_total_sum {get; set;}
    public int no {get; set;}
    public double total_sum {get; set;}
    public bool without_vat {get; set;}
  }

}