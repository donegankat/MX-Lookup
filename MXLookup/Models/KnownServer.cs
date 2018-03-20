using System;
using System.Collections.Generic;
using System.Text;

namespace MXLookup.Models
{
    public class KnownServer
    {
        public string Parent_Server { get; set; } // The name of a parent domain that owns lots of commonly seen MX servers (e.g. google.com and outlook.com). NOTE: Several of these will be the same.
        public string Server_Pattern { get; set; } // The expression used to determing whether a MX record fits a known pattern
    }
}
