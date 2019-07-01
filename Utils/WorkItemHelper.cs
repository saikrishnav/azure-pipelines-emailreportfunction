using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.WebApi;
using System;
using System.Collections.Generic;
using System.Text;

namespace EmailReportFunction.Utils
{
    public static class WorkItemHelper
    {
        public static T GetWorkItemField<T>(this WorkItem workItem, string fieldName)
        {
            object stringDefaultVal = "";
            T defaultRetValue = (typeof(T) == typeof(string)) ? (T)stringDefaultVal : default(T);

            if (workItem.Fields != null)
            {
                IDictionary<string, object> fields = new Dictionary<string, object>(workItem.Fields,
                    StringComparer.CurrentCultureIgnoreCase);

                if (fields.ContainsKey(fieldName))
                {
                    var fieldValue = fields[fieldName];
                    if (fieldValue == null) return defaultRetValue;

                    if (fieldValue is T)
                    {
                        return (T)fieldValue;
                    }
                    else if (typeof(T) == typeof(string))
                    {
                        object stringValue = fieldValue.ToString();
                        return (T)stringValue;
                    }
                }
            }

            // TODO - Log.LogWarning($"Unable to fetch field {fieldName} in workitem - {workItem.Id}");
            return defaultRetValue;
        }

        public static string GetWorkItemAssignedTo(this WorkItem workItem, string fieldName)
        {
            var assignedToRef = workItem.GetWorkItemField<IdentityRef>(fieldName);
            return assignedToRef == null ? string.Empty : assignedToRef.UniqueName;
        }
    }
}
