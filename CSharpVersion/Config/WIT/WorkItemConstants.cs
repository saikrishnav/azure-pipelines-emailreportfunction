using System;
using System.Collections.Generic;
using System.Text;

namespace EmailReportFunction.Config.WIT
{
    public static class WorkItemStateConstants
    {
        public const string Active = "Active";
        public const string Resolved = "Resolved";
    }

    public static class WorkItemConstants
    {
        #region FieldRefNames

        public const string FieldsRef = "/fields/";
        public const string IssueTypeFieldName = "Microsoft.DevDiv.IssueType";
        public const string ResolvedReasonFieldName = "Microsoft.VSTS.Common.ResolvedReason";
        public const string ResolvedByFieldName = "Microsoft.VSTS.Common.ResolvedBy";
        public const string PriorityFieldName = "Microsoft.VSTS.Common.Priority";
        public const string ResolvedDateFieldName = "Microsoft.VSTS.Common.ResolvedDate";
        public const string ReproStepsFieldName = "Microsoft.VSTS.TCM.ReproSteps";
        public const string HitCountFieldName = "Microsoft.DevDiv.HitCount";

        #endregion

        #region FieldRefValues

        public const string StateActive = "Active";
        public const string StateClosed = "Closed";
        public const string ReasonDuplicate = "Duplicate";
        public const string ReasonReactivated = "Reactivated";
        public const string ReasonNotFixed = "Not fixed";
        public const string IssueTypeCodeDefect = "Code Defect";

        #endregion

        #region RelationRelValues

        public const string RelationRelRelated = "System.LinkTypes.Related";
        public const string RelationRelDuplicate = "System.LinkTypes.Duplicate";
        public const string RelationRelDuplicateReverse = "System.LinkTypes.Duplicate-Reverse";
        public const string RelationRelDuplicateForward = "System.LinkTypes.Duplicate-Forward";
        public const string RelationRelHierarchy = "System.LinkTypes.Hierarchy";
        public const string RelationRelHierarchyReverse = "System.LinkTypes.Hierarchy-Reverse";
        public const string RelationRelHierarchyForward = "System.LinkTypes.Hierarchy-Forward";
        public const string RelationRelDependency = "System.LinkTypes.Dependency";
        public static readonly HashSet<string> WorkItemLinkTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            RelationRelRelated,
            RelationRelHierarchy,
            RelationRelHierarchyReverse,
            RelationRelHierarchyForward,
            RelationRelDependency,
            RelationRelDuplicate,
            RelationRelDuplicateReverse,
            RelationRelDuplicateForward,
        };

        #endregion

        #region CommentValues

        public const string CommentMessage = "</a> <br><b><font color=\"red \">Important: </font><br> Please include the resolving commit as a link to the bug for reliability run to mark the test result as reliable. If the fix was not a commit, please attach any commit when resolving.</b>";
        public const string AlertMessage = "Reliability Bug Alert";
        public const string P0AlertMessage = "P0 test, hot hand off is required.";
        public const string HotHandOffMessage = "The automated reliability system is bumping up the priority of the bug to P0 as it has crossed the hit count threshold (default = 10). Please treat this as a hot hand-off for P0 bug.";
        public const string WikiUrl = "https://dev.azure.com/mseng/AzureDevOps/_wiki/wikis/AzureDevOps.wiki?wikiVersion=GBwikiMaster&pagePath=%2FOrphaned%20pages%2FTracking%20Test%20Reliability";
        public static readonly string BugCreationComment = $"This is an automated reliability system bug created for flaky test and details are present in repro steps pane. For more info visit : <a href={WikiUrl}> {WikiUrl} </a> .<br><br> <b><font color=\"red \">Important: </font><br> For details please look at Repro Steps section. Also, please make sure to port your fix to all relevant active branches to prevent failures.</b><br>";
        public const string ResolvedAsFixed = "Fixed";

        #endregion
    }
}
