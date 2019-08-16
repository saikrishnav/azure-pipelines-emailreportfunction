using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace EmailReportFunction.Utils
{
    public static class StringUtils
    {
        public static string CompressNewLines(string content)
        {
            if (content != null)
            {
                IEnumerable<string> lines = GetNonEmptyLines(content);

                content = string.Join("\n", lines);
            }

            return content;
        }

        public static string ReplaceNewlineWithBrTag(string content)
        {
            if (content == null)
            {
                return null;
            }

            IEnumerable<string> lines = GetNonEmptyLines(content);

            return string.Join("<br/>", lines);
        }

        public static string GetFirstNLines(string content, int lineCount)
        {
            if (content != null)
            {
                var lines = GetNonEmptyLines(content);
                return string.Join("\n", lines.Take(lineCount));
            }
            return null;
        }

        private static IEnumerable<string> GetNonEmptyLines(string s)
        {
            s = s.Replace("\r", "");
            return s.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Where(str => !string.IsNullOrWhiteSpace(str))
                .Select(str => str.Trim());
        }

        public static string GetXmlValidString(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return text;
            }

            return s_xmlCharactersToRemove.Replace(text, "");
        }

        public static bool CompareIgnoreCase(this string str1, string str2)
        {
            return string.Compare(str1, str2, StringComparison.OrdinalIgnoreCase) == 0;
        }

        public static string Truncate(this string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return value;
            return value.Length <= maxLength ? value : value.Substring(0, maxLength);
        }

        public static bool IsNumber(string s)
        {
            ulong number;
            return ulong.TryParse(s, out number);
        }

        private static readonly Regex s_xmlCharactersToRemove = new Regex("[\x00-\x08\x0B\x0C\x0E-\x1F]", RegexOptions.Compiled);
    }
}
