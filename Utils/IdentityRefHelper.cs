using Microsoft.VisualStudio.Services.WebApi;
using System;
using System.Collections.Generic;
using System.Text;

namespace EmailReportFunction.Utils
{
    public static class IdentityRefHelper
    {
        public static string GetUniqueName(IdentityRef identity)
        {
            return string.IsNullOrWhiteSpace(identity.UniqueName)
                ? identity.DisplayName
                : identity.UniqueName;
        }

        public static bool IsValid(IdentityRef identity)
        {
            return identity != null &&
                   (!string.IsNullOrWhiteSpace(identity.DisplayName) ||
                    !string.IsNullOrWhiteSpace(identity.UniqueName));

        }

        public static string GetMailAddress(IdentityRef identity)
        {
            if (!identity.IsContainer)
            {
                return GetUniqueName(identity);
            }
            return null;
        }
    }
}
