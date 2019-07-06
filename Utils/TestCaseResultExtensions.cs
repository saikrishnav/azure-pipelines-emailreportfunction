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

        private static CustomTestField GetCustomField(this TestCaseResult result, string fieldName)
        {
            if (result.CustomFields == null)
            {
                return null;
            }

            var cf = result.CustomFields.FirstOrDefault(c => c.FieldName.Equals(fieldName, StringComparison.OrdinalIgnoreCase));
            return cf;
        }
    }
}
