using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using EmailReportFunction.Config;
using EmailReportFunction.Config.Pipeline;
using EmailReportFunction.Utils;
using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.TestManagement.WebApi;
using Microsoft.VisualStudio.Services.Common;

namespace Microsoft.EmailTask.EmailReport.ViewModel
{
    public class MailAddressViewModel
    {
        public List<MailAddress> Cc { get; set; }

        public MailAddress From { get; set; }
        public List<MailAddress> To { get; set; }

        private ILogger _logger;

        public MailAddressViewModel(ILogger logger)
        {
            _logger = logger;
        }

        public async Task InitializeViewModel(IPipelineData pipelineData, EmailReportConfiguration emailReportConfig)
        {
            From = new MailAddress(emailReportConfig.SmtpConfiguration.UserName);
            _logger.LogInformation("computing email addresses for to section");
            To = await GetMailAddressesAsync(pipelineData, emailReportConfig.To);
            _logger.LogInformation("computing email addresses for Cc section");
            Cc = await GetMailAddressesAsync(pipelineData, emailReportConfig.Cc);
        }

        #region Helpers

        private IEnumerable<string> GetEmailAddressesFromString(string mailAddresses)
        {
            var mailAddressSet = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);

            if (string.IsNullOrEmpty(mailAddresses))
            {
                _logger.LogInformation("default mail address not set");
            }
            else
            {
                string[] addresses = mailAddresses.Split(new[] {';'}, StringSplitOptions.RemoveEmptyEntries);

                foreach (var address in addresses)
                {
                    if (!string.IsNullOrWhiteSpace(address))
                    {
                        mailAddressSet.Add(address);
                    }
                }
            }

            return mailAddressSet;
        }

        private async Task<List<MailAddress>> GetMailAddressesAsync(IPipelineData pipelineData,
            MailRecipientsConfiguration recipientsConfiguration)
        {
            var addressHashSet = new HashSet<string>(StringComparer.CurrentCultureIgnoreCase);

            if (recipientsConfiguration.IncludeTestOwners)
            {
                addressHashSet.AddRange(GetFailedTestOwners(pipelineData));
            }

            if (recipientsConfiguration.IncludeActiveBugOwners)
            {
                addressHashSet.AddRange(GetActiveBugOwnersForFailedTests(pipelineData));
            }

            if (recipientsConfiguration.IncludeChangesetOwners)
            {
                var associatedChanges = await pipelineData.GetAssociatedChangesAsync();
                addressHashSet.AddRange(GetChangesetOwners(associatedChanges));
            }

            if (recipientsConfiguration.IncludeCreatedBy)
            {
                addressHashSet.Add(GetCreatedBy(pipelineData));
            }
            addressHashSet.AddRange(GetEmailAddressesFromString(recipientsConfiguration.DefaultRecipients));

            return GetMailAddresses(addressHashSet)
                .DistinctBy(mailAddress => mailAddress.Address.ToLowerInvariant())
                .ToList();
        }

        private async Task<List<string>> GetFailedTestOwnersAsync(IPipelineData pipelineData)
        {
            var mailAddresses = new List<string>();
            var failedTestOwners = await pipelineData.GetFailedTestOwnersAsync();
            failedTestOwners.ForEach(identity =>
            {
                var mailAddress = IdentityRefHelper.GetMailAddress(identity);
                if (!string.IsNullOrWhiteSpace(mailAddress))
                {
                    mailAddresses.Add(mailAddress);
                }
            });

            _logger.LogInformation($"Failed Test owners - {string.Join(",", mailAddresses)}");
            return mailAddresses;
        }

        private List<string> GetActiveBugOwnersForFailedTests(IPipelineData pipelineData)
        {
            if (pipelineData.FilteredResults == null)
            {
                return new List<string>();
            }

            List<string> bugOwners = pipelineData.FilteredResults
                .Where(group => group.TestResults.ContainsKey(TestOutcome.Failed))
                .SelectMany(group => group.TestResults[TestOutcome.Failed])
                .SelectMany(result => result.AssociatedBugs)
                .Where(
                    bug =>
                        string.Equals(
                            bug.GetWorkItemField<string>(DatabaseCoreFieldRefName.State),
                            WorkItemStateConstants.Active,
                            StringComparison.CurrentCultureIgnoreCase))
                .Select(bug => bug.GetWorkItemAssignedTo(DatabaseCoreFieldRefName.AssignedTo))
                .ToList();

            _logger.LogInformation($"Failed Test owners - {string.Join(",", bugOwners)}");
            return bugOwners;
        }

        private List<string> GetChangesetOwners(List<ChangeData> associatedChanges)
        {
            var mailAddresses = new List<string>();
            if (associatedChanges?.Any() != true)
            {
                _logger.LogInformation("No changeset owner mail addresses");
                return mailAddresses;
            }

            foreach (var associatedChange in associatedChanges)
            {
                var mailAddress = associatedChange.Author.UniqueName;
                if (string.IsNullOrWhiteSpace(mailAddress))
                {
                    _logger.LogWarning($"Unable to get mail address for associated change - {associatedChange.Id}");
                }
                else
                {
                    mailAddresses.Add(mailAddress);
                }
            }

            _logger.LogInformation($"Changeset owner mail addresses - {string.Join(",", mailAddresses)}");
            return mailAddresses;
        }

        private string GetCreatedBy(IPipelineData pipelineData)
        {
            if (pipelineData.CreatedBy == null)
            {
                throw new Exception("Unexpected error - CreatedBy found null");
            }

            return pipelineData.CreatedBy.UniqueName;
        }

        private List<MailAddress> GetMailAddresses(HashSet<string> addressHashSet)
        {
            var mailAddresses = new List<MailAddress>();
            foreach (var address in addressHashSet)
            {
                var validAddress = GetValidEmailAddress(address);
                if (!string.IsNullOrWhiteSpace(validAddress))
                {
                    mailAddresses.Add(new MailAddress(validAddress));
                }
            }
            return mailAddresses;
        }

        private string GetValidEmailAddress(string address)
        {
            if (!string.IsNullOrWhiteSpace(address) && !IsValidEmail(address))
            {
                //TODO Need to add support maillAddressCollection get user information from AAD
                _logger.LogInformation($"Email address {address} is not valid. Adding @microsoft.com");
                address = $"{address}@microsoft.com";
            }

            return address;
        }

        private static bool IsValidEmail(string email)
        {
            try
            {
                // ReSharper disable once ObjectCreationAsStatement
                new MailAddress(email);
            }
            catch (Exception e) when (e is ArgumentException || e is FormatException)
            {
                return false;
            }

            return true;
        }

        #endregion
    }
}