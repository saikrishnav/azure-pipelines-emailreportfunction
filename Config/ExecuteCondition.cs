using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace EmailReportFunction.Config
{
    [DataContract]
    public class ExecuteCondition
    { 
        [DataMember]
        public bool ConditionEnabled { get; set; }

        [DataMember]
        public string ComparisonType { get; set; }

        [DataMember]
        public string CurrentValue { get; set; }

        [DataMember]
        public string ExpectedValue { get; set; }

        internal bool Evaluate()
        {
            if(string.Equals(ComparisonType, "eq", StringComparison.OrdinalIgnoreCase))
            {
                return string.Equals(CurrentValue, ExpectedValue, StringComparison.OrdinalIgnoreCase);
            }
            else if (string.Equals(ComparisonType, "ne", StringComparison.OrdinalIgnoreCase))
            {
                return !string.Equals(CurrentValue, ExpectedValue, StringComparison.OrdinalIgnoreCase);
            }
            else if (string.Equals(ComparisonType, "in", StringComparison.OrdinalIgnoreCase))
            {
                var expectedValues = ExpectedValue.Split(',');
                return expectedValues.Any(e => string.Equals(CurrentValue, e, StringComparison.OrdinalIgnoreCase));
            }
            return false;
        }
    }
}
