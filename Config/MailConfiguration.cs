using EmailReportFunction.Config;

namespace EmailReportFunction.Config
{
    public class MailConfiguration
    {
        public SendMailCondition SendMailCondition { get; set; }
        public string ToAddrresses { get; set; }
        public string IncludeInTo { get; set; }
        public string CcAddrresses { get; set; }
        public string IncludeInCc { get; set; }
    }
}