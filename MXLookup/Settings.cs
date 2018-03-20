using MXLookup.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace MXLookup
{
    public class Settings
    {
        public List<KnownServer> KnownServers { get; set; }

        public Settings()
        {
            KnownServers = new List<KnownServer>();
        }
    }
}
