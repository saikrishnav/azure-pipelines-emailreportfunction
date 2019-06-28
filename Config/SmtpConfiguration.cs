using System;
using System.Collections.Generic;
using System.Text;

namespace EmailReportFunction.Config
{
    public class SmtpConfiguration
    {
        public string UserName { get; set; }

        public string Password { get; set; }

        public string SmtpHost { get; set; }

        public bool EnableSSL { get; set; }
    }
}
