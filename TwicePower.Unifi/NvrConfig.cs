using System;
using System.Collections.Generic;
using System.Text;

namespace TwicePower.Unifi
{
    public class NvrConfig
    {
        public string BaseUrl { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public bool VerifySsl { get; set; } = true;
        public string SocksProxy { get; set; }

    }
}
