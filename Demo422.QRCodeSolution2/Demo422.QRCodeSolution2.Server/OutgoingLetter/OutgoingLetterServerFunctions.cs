using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Demo422.QRCodeSolution2.OutgoingLetter;

namespace Demo422.QRCodeSolution2.Server
{
  partial class OutgoingLetterFunctions
  {
    [Remote, Public]
    public string GetPublicBodyID()
    {
      string bodyId="";
      using (var command = SQL.GetCurrentConnection().CreateCommand())
      {
        command.CommandText = string.Format(Queries.OutgoingLetter.GetHashCode, _obj.Id);
        var obj=command.ExecuteScalar();
        bodyId=obj.ToString();
      }
      return bodyId;
    }
  }
}