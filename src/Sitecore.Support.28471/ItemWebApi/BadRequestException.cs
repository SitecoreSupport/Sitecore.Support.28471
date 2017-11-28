using Sitecore.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sitecore.Support.ItemWebApi
{
  internal class BadRequestException : Exception
  {
    public BadRequestException(string message) : base(message)
    {
      Assert.ArgumentNotNull(message, "message");
    }
  }
}