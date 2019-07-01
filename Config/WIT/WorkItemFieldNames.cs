using System;
using System.Collections.Generic;
using System.Text;

namespace EmailReportFunction.Config.WIT
{
    public static class WorkItemCoreFieldRefNames
    {
        public const string AreaId = "System.AreaId";
        public const string IterationPath = "System.IterationPath";
        public const string LinkType = "System.Links.LinkType";
        public const string NodeName = "System.NodeName";
        public const string Reason = "System.Reason";
        public const string RelatedLinkCount = "System.RelatedLinkCount";
        public const string Rev = "System.Rev";
        public const string IterationId = "System.IterationId";
        public const string RevisedDate = "System.RevisedDate";
        public const string AuthorizedDate = "System.AuthorizedDate";
        public const string TeamProject = "System.TeamProject";
        public const string Tags = "System.Tags";
        public const string Title = "System.Title";
        public const string WorkItemType = "System.WorkItemType";
        public const string Watermark = "System.Watermark";
        public const string State = "System.State";
        public const string Id = "System.Id";
        public const string RemoteLinkCount = "System.RemoteLinkCount";
        public const string HyperLinkCount = "System.HyperLinkCount";
        public const string AreaPath = "System.AreaPath";
        public const string AssignedTo = "System.AssignedTo";
        public const string AttachedFileCount = "System.AttachedFileCount";
        public const string AuthorizedAs = "System.AuthorizedAs";
        public const string BoardColumn = "System.BoardColumn";
        public const string BoardColumnDone = "System.BoardColumnDone";
        public const string BoardLane = "System.BoardLane";
        public const string ChangedBy = "System.ChangedBy";
        public const string ChangedDate = "System.ChangedDate";
        public const string CreatedBy = "System.CreatedBy";
        public const string CreatedDate = "System.CreatedDate";
        public const string Description = "System.Description";
        public const string CommentCount = "System.CommentCount";
        public const string ExternalLinkCount = "System.ExternalLinkCount";
        public const string History = "System.History";
        public const string IsDeleted = "System.IsDeleted";

        public static IEnumerable<string> All { get; }
    }
}
