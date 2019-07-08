using EmailReportFunction.Config;
using EmailReportFunction.Utils;
using Microsoft.TeamFoundation.TestManagement.WebApi;
using Microsoft.VisualStudio.Services.ReleaseManagement.WebApi.Contracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace EmailReportFunction.ViewModel.Helpers
{
    public static class LinkHelper
    {
        private const string TcmPipelineExtension = "_TestManagement/Runs";

        // Release related strings
        private const string ReleaseProgressView = "_releaseProgress";
        private const string ReleaseDefView = "_releaseDefinition";
        private const string ReleaseEnvironmentExtension = "release-environment-extension";
        private const string ReleaseLinkTestExtensionId = "ms.vss-test-web.test-result-in-release-environment-editor-tab";

        private const string WorkItemPipelineExtension = "_workitems";
        private const string BuildPipelineExtension = "_build";

        public static Uri GetBaseUri(string uri)
        {
            // If base Uri does not end with "/", new Uri(collectionUri, webAccessRelativePath) constructor 
            // removes the defaultCollection part from the Uri.
            var baseUri = uri + (uri.EndsWith("/", StringComparison.OrdinalIgnoreCase) ? string.Empty : "/");

            return new Uri(baseUri);
        }

        public static string GetBuildDefinitionLink(Artifact artifact, PipelineConfiguration config)
        {
            return GetBuildDefinitionLink(
                ArtifactHelper.GetArtifactInfo(artifact, ArtifactDefinitionConstants.Definition)?.Id,
                config);
        }

        public static string GetBuildDefinitionLink(string definitionId, PipelineConfiguration config)
        {
            var collectionUri = config.ServerUri;
            var parameters = new Dictionary<string, object>
            {
                {"definitionId", definitionId},
                {"_a", "completed"}
            };
            var uri = GetBuildLink(config, collectionUri, parameters);

            return uri;
        }

        public static string GetBuildSummaryLink(Artifact artifact, PipelineConfiguration config)
        {
            return GetBuildSummaryLink(
                ArtifactHelper.GetArtifactInfo(artifact, ArtifactDefinitionConstants.Version)?.Id,
                config);
        }

        public static string GetBuildSummaryLink(string buildId, PipelineConfiguration config)
        {
            var collectionUri = config.ServerUri;
            var parameters = new Dictionary<string, object>
            {
                {"buildId", buildId},
                {"_a", "summary"}
            };
            var uri = GetBuildLink(config, collectionUri, parameters);

            return uri;
        }

        //TODO
        //public static string GetTestTabLinkInBuild(Config.BuildConfiguration buildConfig,
        //    Dictionary<string, string> queryParams = null)
        //{
        //    var collectionUri = buildConfig.ServerUri;
        //    var parameters = new Dictionary<string, object>
        //    {
        //        {"buildId", buildConfig.BuildId},
        //        {"_a", "summary"},
        //        {"tab","ms.vss-test-web.test-result-details" }
        //    };

        //    if (queryParams != null)
        //    {
        //        foreach (var key in queryParams.Keys)
        //        {
        //            parameters[key] = queryParams[key];
        //        }
        //    }

        //    var uri = GetBuildLink(buildConfig, collectionUri, parameters);

        //    return uri;
        //}

        public static string GetCommitLink(string changeId, string changeUri, PipelineConfiguration config)
        {
            var collectionUri = config.ServerUri;
            string repoId = null;
            var pos = changeUri.IndexOf("repositories", StringComparison.InvariantCultureIgnoreCase);
            if (pos > 0)
            {
                repoId =
                    changeUri.Substring(pos).Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries)[1];
            }

            var uri = new Uri(GetBaseUri(collectionUri),
                $"{config.ProjectName}/_git/{repoId}/commit/{changeId}").AbsoluteUri;
            return uri;
        }

        public static string GetCreateBugLinkForTest(PipelineConfiguration config,
            TestCaseResult testResult)
        {
            return GetTestResultLink(config, testResult.TestRun?.Id,
                testResult.Id, new Dictionary<string, string>
                {
                    {"create-bug", "true"}
                });
        }

        public static string GetQueryParameter(Dictionary<string, object> parameterValues)
        {
            var queryString = string.Empty;
            foreach (var key in parameterValues.Keys)
            {
                queryString = string.IsNullOrEmpty(queryString)
                    ? "?" + key + "=" + parameterValues[key]
                    : queryString + "&" + key + "=" + parameterValues[key];
            }

            return queryString;
        }

        public static string GetReleaseDefinitionLink(PipelineConfiguration config,
            int releaseDefinitionId)
        {
            var collectionUri = config.ServerUri;
            var parameters = GetQueryParameter(new Dictionary<string, object>
            {
                {"definitionId", releaseDefinitionId},
                {"_a", "environments-editor"}
            });

            var uri = new Uri(GetBaseUri(collectionUri), config.ProjectName + "/" + ReleaseDefView + parameters)
                .AbsoluteUri;

            return uri;
        }

        public static string GetReleaseLogsTabLink(ReleaseConfiguration releaseConfig)
        {
            return GetReleaseLinkTab(releaseConfig, "release-logs");
        }

        public static string GetReleaseSummaryLink(PipelineConfiguration config)
        {
            return GetReleaseLinkTab(config, "release-summary");
        }

        public static string GetTestResultLink(PipelineConfiguration config, string runId, int resultId,
            Dictionary<string, string> queryParams = null)
        {
            var collectionUri = config.ServerUri;
            var parameters = new Dictionary<string, object>
            {
                {"runId", runId},
                {"_a", "resultSummary"},
                {"resultId", resultId}
            };

            if (queryParams != null)
            {
                foreach (KeyValuePair<string, string> keyValuePair in queryParams)
                {
                    parameters.Add(keyValuePair.Key, keyValuePair.Value);
                }
            }

            var uri = new Uri(GetBaseUri(collectionUri),
                GetTcmRelativeUrl(config.ProjectName) + GetQueryParameter(parameters)).AbsoluteUri;

            return uri;
        }

        public static string GetTestTabLinkInRelease(ReleaseConfiguration releaseConfig)
        {
            return GetReleaseLinkTab(releaseConfig, "ms.vss-test-web.test-result-in-release-environment-editor-tab");
        }

        public static string GetWorkItemLink(PipelineConfiguration config, int workItemId)
        {
            var queryParams = new Dictionary<string, object>
            {
                {"id", workItemId}
            };
            return new Uri(
                GetBaseUri(config.ServerUri),
                GetWorkItemRelativeUrl(config.ProjectName) + GetQueryParameter(queryParams))
                .AbsoluteUri;
        }

        private static ArtifactSourceReference GetArtifactInfo(Artifact artifact, string key)
        {
            return ArtifactHelper.GetArtifactInfo(artifact, key);
        }

        private static string GetBuildLink(PipelineConfiguration config, string collectionUri,
            Dictionary<string, object> parameters)
        {
            var uri = new Uri(GetBaseUri(collectionUri),
                GetBuildRelativeUrl(config.ProjectName) + GetQueryParameter(parameters))
                .AbsoluteUri;
            return uri;
        }

        private static string GetBuildRelativeUrl(string projectName)
        {
            return projectName + "/" + BuildPipelineExtension;
        }

        private static string GetReleaseLinkTab(PipelineConfiguration config, string tab)
        {
            var collectionUri = config.ServerUri;
            var releaseConfig = config as ReleaseConfiguration;
            var parameters = GetQueryParameter(new Dictionary<string, object> {
                { "_a", ReleaseEnvironmentExtension },
                { "releaseId", releaseConfig.ReleaseId },
                { "environmentId", releaseConfig.EnvironmentId },
                { "extensionId", tab } });

            return new Uri(GetBaseUri(collectionUri), config.ProjectName + "/" + ReleaseProgressView + parameters).AbsoluteUri;
        }

        private static string GetTcmRelativeUrl(string projectName)
        {
            return projectName + "/" + TcmPipelineExtension;
        }

        private static string GetWorkItemRelativeUrl(string projectName)
        {
            return projectName + "/" + WorkItemPipelineExtension;
        }
    }
}
