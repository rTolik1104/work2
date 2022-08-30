using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace micros.MicrosSetting.Structures.Module
{
  partial class TokenList
  {
    public string token_type { get; set; }
    public int expires_in { get; set; }
    public string access_token { get; set; }
    public string refresh_token { get; set; }

    public string ErrorMsg { get; set; }
    public DateTime? DateCreate { get; set; }

    public string KeyId { get; set; }
  }
}