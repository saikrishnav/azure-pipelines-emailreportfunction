using EmailReportFunction.Config.Pipeline;
using EmailReportFunction.Config.TestResults;
using EmailReportFunction.Config.WIT;
using Microsoft.TeamFoundation.TestManagement.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.WebApi.Patch.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Operation = Microsoft.VisualStudio.Services.WebApi.Patch.Operation;

namespace EmailReportFunction.Utils
{
    public static class TestCaseResultExtensions
    {
        public static List<TestCaseResult> GetFlakyTestResults(this List<TestCaseResult> results)
        {
            var flakyTestResults = new List<TestCaseResult>();
            foreach (var result in results)
            {
                if (result.IsTestFlaky())
                {
                    flakyTestResults.Add(result);
                }
            }
            return flakyTestResults;
        }

        public static bool IsTestFlaky(this TestCaseResult result)
        {
            var outcomeConfidenceField = result.GetCustomField(TestResultFieldNameConstants.OutcomeConfidence);
            if (outcomeConfidenceField != null
                && outcomeConfidenceField.Value != null
                && float.TryParse(outcomeConfidenceField.Value.ToString(), out float ocValue))
            {
                return ocValue == TaskConstants.OutcomeConfidenceValue;
            }

            return false;
        }

        public static List<TestCaseResult> GetFailedTestResults(this List<TestCaseResult> results)
        {
            var failedTestResults = new List<TestCaseResult>();
            var failedResultsBuilder = new StringBuilder();
            foreach (var result in results)
            {
                if (result.IsFailedTest())
                {
                    failedResultsBuilder.AppendLine($"\t  AutomatedTestName:{result.AutomatedTestName}");
                    failedTestResults.Add(result);
                }

            }
            return failedTestResults;
        }

        public static List<TestCaseResult> GetPassedOnRerunResults(this List<TestCaseResult> results)
        {
            var passedOnRerunTests = new List<TestCaseResult>();
            foreach (var result in results)
            {
                if (result.IsPassedOnRerun())
                {
                    passedOnRerunTests.Add(result);
                }
            }

            return passedOnRerunTests;
        }

        public static bool IsPassedOnRerun(this TestCaseResult result)
        {
            var isPassedTest = Enum.TryParse(result.Outcome, out TestOutcome outcomeValue) && outcomeValue == TestOutcome.Passed;
            return isPassedTest && result.ResultGroupType == ResultGroupType.Rerun;
        }

        public static bool HasAbortedTestResults(this List<TestCaseResult> results)
        {
            foreach (var result in results)
            {
                if (Enum.TryParse(result.Outcome, out TestOutcome outcomeValue) && outcomeValue == TestOutcome.Aborted)
                {
                    return true;
                }

            }

            return false;
        }

        public static List<TestCaseResult> IntersectWithRepetitons(this IEnumerable<TestCaseResult> resultsWithRepetition, IEnumerable<TestCaseResult> otherResults)
        {
            var uniqueResults = new HashSet<string>(otherResults.Select(t => t.GetAutomatedTestName()), StringComparer.OrdinalIgnoreCase);
            return resultsWithRepetition.Where(t => uniqueResults.Contains(t.GetAutomatedTestName())).ToList();
        }

        public static List<TestCaseResult> IntersectWithRepetitons(this IEnumerable<TestCaseResult> resultsWithRepetition, IEnumerable<string> otherResultsTestName)
        {
            var uniqueResults = new HashSet<string>(otherResultsTestName, StringComparer.OrdinalIgnoreCase);
            return resultsWithRepetition.Where(t => uniqueResults.Contains(t.GetAutomatedTestName())).ToList();
        }

        public static List<TestCaseResult> UnionWithRepetitions(this List<TestCaseResult> resultsWithRepetition, List<TestCaseResult> otherResults)
        {
            var uniqueResults = new HashSet<string>(resultsWithRepetition.Select(t => t.GetAutomatedTestName()), StringComparer.OrdinalIgnoreCase);
            var requiredResults = otherResults.Where(t => !uniqueResults.Contains(t.GetAutomatedTestName()));
            resultsWithRepetition.AddRange(requiredResults);
            return resultsWithRepetition;
        }

        public static bool IsFailedTest(this TestCaseResult result)
        {
            return (Enum.TryParse(result.Outcome, out TestOutcome outcomeValue) && outcomeValue == TestOutcome.Failed);
        }

        public static void TagFlakyTests(this IEnumerable<TestCaseResult> results)
        {
            foreach (var result in results)
            {
                if (!result.IsTestFlaky())
                {
                    if (result.CustomFields == null)
                    {
                        result.CustomFields = new List<CustomTestField>();
                    }

                    result.CustomFields.Add(new CustomTestField { FieldName = TestResultFieldNameConstants.OutcomeConfidence, Value = TaskConstants.OutcomeConfidenceValue });
                }
            }
        }

        public static void UntagFlakyTests(this List<TestCaseResult> results)
        {
            foreach (var result in results)
            {
                if (result.CustomFields == null)
                {
                    result.CustomFields = new List<CustomTestField>();
                }

                result.CustomFields.Add(new CustomTestField { FieldName = TestResultFieldNameConstants.OutcomeConfidence, Value = 100.0f });
            }
        }

        public static string GetAutomatedTestName(this TestCaseResult result)
        {
            return GetAutomatedTestName(result.AutomatedTestStorage, result.AutomatedTestName, result.TestCaseTitle);
        }

        public static string GetAutomatedTestName(this TestResultMetaData testMethod)
        {
            return GetAutomatedTestName(testMethod.AutomatedTestStorage, testMethod.AutomatedTestName, testMethod.TestCaseTitle);
        }

        public static WorkItemDocument ToWorkItem(this TestResultInfo testResultInfo, string areaPath, PipelineInputs pipelineInputs, 
            string iterationPath, List<BugRecipientIdentity> recipients, string workItemType = TaskConstants.WorkItemTypeBug, string discussion = "")
        {
            var workItemObject = new WorkItemDocument();

            workItemObject.Type = workItemType;

            var document = new JsonPatchDocument();
            var titleOutcome = testResultInfo.PassedOnRerun ? "passed-on-rerun" : "failed";
            string title = $"{pipelineInputs.TargetWorkflowName} - {testResultInfo.AutomatedTestName} {titleOutcome}";
            if (pipelineInputs.TargetEnvironmentName != null && pipelineInputs.TargetEnvironmentName.Length > 0)
            {
                title = $"{pipelineInputs.TargetWorkflowName} - {pipelineInputs.TargetEnvironmentName} - {testResultInfo.AutomatedTestName} {titleOutcome}";
            }

            if (title.Length > TaskConstants.MaxTitleLength)
            {
                title = title.Substring(0, TaskConstants.MaxTitleLength) + "...";
            }

            document.Add(new JsonPatchOperation
            {
                Operation = Operation.Add,
                Path = ($"{WorkItemConstants.FieldsRef}{WorkItemCoreFieldRefNames.Title}"),
                Value = title
            });

            document.Add(new JsonPatchOperation
            {
                Operation = Operation.Add,
                Path = ($"{WorkItemConstants.FieldsRef}{WorkItemCoreFieldRefNames.AreaPath}"),
                Value = areaPath
            });

            if (pipelineInputs.ReliabilityBugTags != null && pipelineInputs.ReliabilityBugTags.Any())
            {
                document.Add(new JsonPatchOperation
                {
                    Operation = Operation.Add,
                    Path = ($"{WorkItemConstants.FieldsRef}{WorkItemCoreFieldRefNames.Tags}"),
                    Value = string.Join(TaskConstants.TagsDelimeter, pipelineInputs.ReliabilityBugTags)
                });
            }

            document.Add(new JsonPatchOperation
            {
                Operation = Operation.Add,
                Path = ($"{WorkItemConstants.FieldsRef}{WorkItemCoreFieldRefNames.IterationPath}"),
                Value = iterationPath
            });

            string analysis = "<br><b>Analysis:</b>" + discussion;
            var historyValue = WorkItemConstants.BugCreationComment + analysis;
            if (recipients != null && recipients.Any())
            {
                historyValue = WorkItemConstants.BugCreationComment + GetUpdatedBugCommentValue(recipients, WorkItemConstants.AlertMessage) + analysis;
                if (testResultInfo.Priority == 0)
                {
                    historyValue = WorkItemConstants.BugCreationComment + GetUpdatedBugCommentValue(recipients, WorkItemConstants.P0AlertMessage) + analysis;
                }
            }

            document.Add(new JsonPatchOperation
            {
                Operation = Operation.Add,
                Path = $"{WorkItemConstants.FieldsRef}{WorkItemCoreFieldRefNames.History}",
                Value = historyValue
            });

            //Create relationship to test object 
            WorkItemRelation testRelation = new WorkItemRelation
            {
                Rel = "ArtifactLink",
                Url = testResultInfo.GetTestUrl(),
                Attributes = new Dictionary<string, object> { { TaskConstants.AttributeKeyName, TaskConstants.AttributeValueTest } }
            };

            document.Add(new JsonPatchOperation
            {
                Operation = Operation.Add,
                Path = "/relations/-",
                Value = testRelation
            });

            if (!pipelineInputs.SkipVSTSWorkItemFields)
            {
                document.Add(new JsonPatchOperation
                {
                    Operation = Operation.Add,
                    Path = $"{WorkItemConstants.FieldsRef}{WorkItemConstants.PriorityFieldName}",
                    Value = Math.Min(Math.Max(testResultInfo.Priority, 1), 3)
                });

                document.Add(new JsonPatchOperation
                {
                    Operation = Operation.Add,
                    Path = $"{WorkItemConstants.FieldsRef}{WorkItemConstants.IssueTypeFieldName}",
                    Value = WorkItemConstants.IssueTypeCodeDefect
                });

                document.Add(new JsonPatchOperation
                {
                    Operation = Operation.Add,
                    Path = $"{WorkItemConstants.FieldsRef}{WorkItemConstants.ReproStepsFieldName}",
                    Value = GetWorkItemReproStepsMessage(testResultInfo, pipelineInputs.WebUrl)
                });
            }

            workItemObject.Properties = document;
            return workItemObject;
        }

        public static string GetUpdatedBugCommentValue(List<BugRecipientIdentity> recipients, string alertMessage)
        {
            var requiredString = "<br>";
            try
            {
                foreach (var user in recipients)
                {
                    requiredString += $"<a href=’#’ data-vss-mention=’version:2.0,{user.Id}>@{user.Name}</a>";
                }
            }
            catch (Exception)
            {
                // TODO - Log.LogError($"Exception encountered in building mentions format: {e.ToString()}");
            }

            return $"{requiredString} {alertMessage}";
        }

        private static string GetTestUrl(this TestResultInfo testCaseResult)
        {
            return $"vstfs:///TestManagement/TcmTest/tcm.{testCaseResult.TestCaseReferenceId}";
        }

        private static string GetWorkItemReproStepsMessage(TestResultInfo testResultInfo, string webUrl)
        {
            StringBuilder reproStepsMessage = new StringBuilder();
            reproStepsMessage.Append($"Test: <br> <b>{testResultInfo.AutomatedTestName}</b><br>");
            reproStepsMessage.Append($"Build/Release: <br> <b><a href = \"{webUrl}\">{webUrl}</a></b><br>");
            reproStepsMessage.Append($"Test Attachments:<br>");
            for (int i = 0; i < testResultInfo.TestAttachments.Count; i++)
            {
                reproStepsMessage.Append($"\t<br> <b><a href = \"{testResultInfo.TestAttachments[i].Url}\">{testResultInfo.TestAttachments[i].FileName}_{i}</a></b><br>");
            }
            reproStepsMessage.Append($"Owner: <br> <b>{testResultInfo.Owner}</b><br>");
            reproStepsMessage.Append($"Error Message: <br> <b>{WebUtility.HtmlEncode(testResultInfo.ErrorMessage)}</b><br>");
            reproStepsMessage.Append($"Stack Trace: <br>{testResultInfo.StackTrace}<br>");
            return reproStepsMessage.ToString();
        }

        private static CustomTestField GetCustomField(this TestCaseResult result, string fieldName)
        {
            if (result.CustomFields == null)
            {
                return null;
            }

            var cf = result.CustomFields.FirstOrDefault(c => c.FieldName.Equals(fieldName, StringComparison.OrdinalIgnoreCase));
            return cf;
        }

        private static string GetAutomatedTestName(string automatedTestStorage, string automatedTestName, string testCaseTitle)
        {
            // this is required because the test results APIs are broken and aren't returning result names etc.
            if (string.IsNullOrEmpty(automatedTestName) || string.IsNullOrEmpty(automatedTestStorage))
            {
                throw new ArgumentNullException("testNameOrStorageOrTitle", "Either the test name, or test storage or test title are null or empty");
            }

            if (Path.GetExtension(automatedTestStorage).Equals(".dll", StringComparison.OrdinalIgnoreCase))
            {
                return automatedTestName;
            }

            if (Path.GetExtension(automatedTestStorage).Equals(".ts", StringComparison.OrdinalIgnoreCase))
            {
                return $"{automatedTestStorage}.{testCaseTitle}";
            }

            return automatedTestName;
        }
    }
}
