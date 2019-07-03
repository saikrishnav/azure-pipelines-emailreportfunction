using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using EmailReportFunction.Config;
using EmailReportFunction.Config.Pipeline;
using EmailReportFunction.Config.WIT;
using EmailReportFunction.Utils;
using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.TestManagement.WebApi;
using Microsoft.VisualStudio.Services.Common;

namespace EmailReportFunction.ViewModel
{
    public class MailAddressViewModel
    {
        public MailAddress From { get; set; }

        private ILogger _logger;

        private IPipelineData _pipelineData;

        private MailConfiguration _mailConfiguration;

        public MailAddressViewModel(MailConfiguration mailConfiguration, IPipelineData pipelineData, ILogger logger)
        {
            _logger = logger;
            _pipelineData = pipelineData;
            _mailConfiguration = mailConfiguration;
        }

        public async Task<IDictionary<RecipientType, List<MailAddress>>> GetRecipientAdrressesAsync()
        {
            From = new MailAddress(MailConfiguration.MailSenderAddress);
            _logger.LogInformation("computing email addresses for to/cc section");
            return await GetMailAddressesAsync();
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

        private async Task<Dictionary<RecipientType, List<MailAddress>>> GetMailAddressesAsync()
        {
            var recipientAdrresses = new Dictionary<RecipientType, List<MailAddress>>();
            var toAddressHashSet = new HashSet<string>(StringComparer.CurrentCultureIgnoreCase);
            var ccAddressHashSet = new HashSet<string>(StringComparer.CurrentCultureIgnoreCase);

            if (_mailConfiguration.To.IncludeTestOwners || _mailConfiguration.Cc.IncludeTestOwners)
            {
                var failedTestOwners = await GetFailedTestOwnersAsync();
                if (_mailConfiguration.To.IncludeTestOwners)
                {
                    toAddressHashSet.AddRange(failedTestOwners);
                }
                else
                {
                    ccAddressHashSet.AddRange(failedTestOwners);
                }
            }

            if (_mailConfiguration.To.IncludeActiveBugOwners || _mailConfiguration.Cc.IncludeTestOwners)
            {
                var activeBugOwners = await GetActiveBugOwnersForFailedTestsAsync();
                if (_mailConfiguration.To.IncludeActiveBugOwners)
                {
                    toAddressHashSet.AddRange(activeBugOwners);
                }
                else
                {
                    ccAddressHashSet.AddRange(activeBugOwners);
                }
            }


            if (_mailConfiguration.To.IncludeChangesetOwners || _mailConfiguration.Cc.IncludeChangesetOwners)
            {
                var associatedChanges = await _pipelineData.GetAssociatedChangesAsync();
                var changeSetOwners = GetChangesetOwners(associatedChanges);
                if (_mailConfiguration.To.IncludeActiveBugOwners)
                {
                    toAddressHashSet.AddRange(changeSetOwners);
                }
                else
                {
                    ccAddressHashSet.AddRange(changeSetOwners);
                }
            }

            if (_mailConfiguration.To.IncludeCreatedBy)
            {
                toAddressHashSet.Add(GetCreatedBy());
            }
            else if(_mailConfiguration.Cc.IncludeCreatedBy)
            {
                ccAddressHashSet.Add(GetCreatedBy());
            }

            toAddressHashSet.AddRange(GetEmailAddressesFromString(_mailConfiguration.To.DefaultRecipients));
            ccAddressHashSet.AddRange(GetEmailAddressesFromString(_mailConfiguration.Cc.DefaultRecipients));

            recipientAdrresses.Add(RecipientType.TO, GetMailAddresses(toAddressHashSet)
                .DistinctBy(mailAddress => mailAddress.Address.ToLowerInvariant())
                .ToList());
            recipientAdrresses.Add(RecipientType.CC, GetMailAddresses(ccAddressHashSet)
                .DistinctBy(mailAddress => mailAddress.Address.ToLowerInvariant())
                .ToList());

            return recipientAdrresses;
        }

        private async Task<List<string>> GetFailedTestOwnersAsync()
        {
            var mailAddresses = new List<string>();
            var failedTestOwners = await _pipelineData.GetFailedTestOwnersAsync();
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

        private async Task<List<string>> GetActiveBugOwnersForFailedTestsAsync()
        {
            var filteredTestData = await _pipelineData.GetFilteredTestsAsync();
            if (filteredTestData == null || filteredTestData.FilteredTests == null)
            {
                return new List<string>();
            }

            List<string> bugOwners = filteredTestData.FilteredTests
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

        private string GetCreatedBy()
        {
            if (_pipelineData.CreatedBy == null)
            {
                throw new Exception("Unexpected error - CreatedBy found null");
            }

            return _pipelineData.CreatedBy.UniqueName;
        }

        private List<MailAddress> GetMailAddresses(IEnumerable<string> addressHashSet)
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

    public enum RecipientType
    {
        TO,
        CC
    }
}