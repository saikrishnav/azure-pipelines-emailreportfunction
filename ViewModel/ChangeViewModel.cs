using System;
using System.Runtime.Serialization;
using Microsoft.EmailTask.EmailReport.Config;
using Microsoft.EmailTask.EmailReport.Dto;
using Microsoft.EmailTask.EmailReport.Utils;
using Microsoft.EmailTask.EmailReport.ViewModel.Helpers;

namespace Microsoft.EmailTask.EmailReport.ViewModel
{
    [DataContract]
    public class ChangeViewModel
    {
        public const int ConstHashLength = 8;

        [DataMember]
        public string AuthorName { get; set; }

        [DataMember]
        public string Id { get; set; }

        [DataMember]
        public string Message { get; set; }

        [DataMember]
        public string TimeStamp { get; set; }

        [DataMember]
        public string Url { get; set; }

        [DataMember]
        public string ShortId { get; set; }

        private ChangeViewModel(string id, string message, string authorName, string timeStamp, string url)
        {
            Id = id;
            ShortId = StringUtils.IsNumber(Id) ? Id : Id.Substring(0, ConstHashLength);

            Message = message;
            AuthorName = authorName;
            TimeStamp = timeStamp;

            Url = url;
        }

        public ChangeViewModel(ChangeDto change, BaseConfiguration config)
            : this(change.Id,
                StringUtils.CompressNewLines(change.Message),
                change.Author?.DisplayName,
                DateTimeHelper.GetLocalTimeWithTimeZone(change.Timestamp),
                LinkHelper.GetCommitLink(change.Id, change.Location.AbsoluteUri, config))
        {
        }
    }
}