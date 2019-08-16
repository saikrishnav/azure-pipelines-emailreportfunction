using EmailReportFunction.Exceptions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;

namespace EmailReportFunction.Utils
{
    public static class EnumHelper
    {
        public static string GetDescription(this Enum enumObject)
        {
            if (enumObject == null)
            {
                throw new ArgumentNullException(nameof(enumObject));
            }

            Type type = enumObject.GetType();
            var value = enumObject.ToString();
            FieldInfo field = type.GetField(value);

            if (field != null)
            {
                var attr =
                    Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) as DescriptionAttribute;

                if (attr != null)
                {
                    return attr.Description;
                }
                throw new EmailReportException(
                    $"Enum '{enumObject}' of type '{type}' doesnt have description attribute");
            }
            throw new EmailReportException($"Unable to get field info for enum '{enumObject}' in '{type}'");
        }

        /// <summary>
        ///   Converts enum string say "InProgress" to "In progress"
        /// </summary>
        public static string GetDisplayName(this Enum e)
        {
            var enumStr = e.ToString();

            var chunks = new List<string>();

            for (var pos = 0; pos < enumStr.Length; pos++)
            {
                var currentChar = enumStr[pos];

                if (pos != 0 && char.IsUpper(currentChar))
                {
                    chunks.Add($" {currentChar.ToString().ToLower()}");
                }
                else
                {
                    chunks.Add(currentChar.ToString());
                }
            }
            return string.Concat(chunks);
        }

        // Restricting template (T) to IConvertible (Enum's base class), since we cannot restrict it to Enum
        public static T[] GetEnumsExcept<T>(params T[] enums) where T : struct, IConvertible
        {
            return ((T[])Enum.GetValues(typeof(T)))
                .Where(val => !enums.Contains(val)).ToArray();
        }

        public static T Parse<T>(string value)
        {
            return (T)Enum.Parse(typeof(T), value);
        }
    }
}
