using System.Runtime.Serialization;

namespace EmailReportFunction.ViewModel
{
    [DataContract]
    public class TaskIssueViewModel
    {
        [DataMember]
        public string Message { get; set; }

        [DataMember]
        public string IssueType { get; set; }

        [DataMember]
        public string AgentName { get; set; }

        public TaskIssueViewModel()
        {
            
        }

        public TaskIssueViewModel(string issueMessage, string issueType, string agentName)
        {
            Message = $"({agentName}) {issueMessage.Trim()}";
            IssueType = issueType;
            AgentName = agentName;
        }

    }
}