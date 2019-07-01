using Microsoft.VisualStudio.Services.ReleaseManagement.WebApi;
using System;
using System.Collections.Generic;
using System.Text;

namespace EmailReportFunction.Config.Pipeline
{
    public class TaskConstants
    {
        public const int MaxResultsToFetch = 100;

        public const int MaxReleaseResultsUsingTimeThreshold = 50;

        public const int BadReleaseTestFailureThreshold = 5;

        public const float OutcomeConfidenceValue = 0.0f;

        public const int MaxTitleLength = 251;

        public const string BugIssueTypePath = "/fields/Microsoft.DevDiv.IssueType";

        public const string TagsDelimeter = ";";

        public const string WorkItemTypeBug = "BUG";

        public const string WorkItemCategoryBug = "Microsoft.BugCategory";

        public const string AttributeKeyName = "name";

        public const string AttributeValueTestResult = "Test Result";

        public const string AttributeValueTest = "Test";

        public const int EnvironmentStatusFilter = (int)(EnvironmentStatus.Succeeded | EnvironmentStatus.Rejected | EnvironmentStatus.PartiallySucceeded);

        public const string NameAttribute = "name";

        public const string Build = "build";

        public const string Release = "release";

        public const string EffectiveStateOn = "On";

        public const int HighestWorkItemPriority = 0;

        public const string ErrorIssueType = "error";

        public const string WorkflowPropertyName = "WorkflowName";

        public const string WebUrlPropertyName = "WebUrl";

        public const string ANDSearchQueryConjunction = "AND";

        public static readonly IEnumerable<string> TestFieldsToFetch = new List<string>
        {
            TestResultFieldNameConstants.OutcomeConfidence,
            TestResultFieldNameConstants.Outcome,
            TestResultFieldNameConstants.Owner,
            TestResultFieldNameConstants.TestCaseTitle,
            TestResultFieldNameConstants.Priority,
            TestResultFieldNameConstants.AutomatedTestName,
            TestResultFieldNameConstants.AutomatedTestStorage,
            TestResultFieldNameConstants.ErrorMessage,
            TestResultFieldNameConstants.StackTrace,
            TestResultFieldNameConstants.Url,
            TestResultFieldNameConstants.ResultGroupType
        };
    }

    public static class TestResultFieldNameConstants
    {
        public const string AutomatedTestName = "AutomatedTestName";
        public const string AutomatedTestStorage = "AutomatedTestStorage";
        public const string Owner = "Owner";
        public const string Outcome = "Outcome";
        public const string OutcomeConfidence = "OutcomeConfidence";
        public const string Priority = "Priority";
        public const string TestCaseTitle = "TestCaseTitle";
        public const string StackTrace = "StackTrace";
        public const string ErrorMessage = "ErrorMessage";
        public const string Url = "Url";
        public const string TestCaseReferenceId = "TestCaseReferenceId";
        public const string ResultGroupType = "TestResultGroupType";
    }
}
