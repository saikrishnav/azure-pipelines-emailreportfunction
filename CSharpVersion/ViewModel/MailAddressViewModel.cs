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

        private AbstractReport _reportData;

        private MailConfiguration MailConfiguration => _reportData.MailConfiguration;

        public MailAddressViewModel(AbstractReport reportData, ILogger logger)
        {
            _logger = logger;
            _reportData = reportData;
        }

        public IDictionary<RecipientType, List<MailAddress>> GetRecipientAdrresses()
        {
            From = new MailAddress(MailConfiguration.MailSenderAddress);
            _logger.LogInformation("computing email addresses for to/cc section");
            return GetMailAddresses();
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

        private IDictionary<RecipientType, List<MailAddress>> GetMailAddresses()
        {
            var recipientAdrresses = new Dictionary<RecipientType, List<MailAddress>>();
            var toAddressHashSet = new HashSet<string>(StringComparer.CurrentCultureIgnoreCase);
            var ccAddressHashSet = new HashSet<string>(StringComparer.CurrentCultureIgnoreCase);

            if (MailConfiguration.To.IncludeTestOwners || MailConfiguration.Cc.IncludeTestOwners)
            {
                var failedTestOwners = GetFailedTestOwners();
                if (MailConfiguration.To.IncludeTestOwners)
                {
                    toAddressHashSet.AddRange(failedTestOwners);
                }
                else
                {
                    ccAddressHashSet.AddRange(failedTestOwners);
                }
            }

            if (MailConfiguration.To.IncludeActiveBugOwners || MailConfiguration.Cc.IncludeTestOwners)
            {
                var activeBugOwners = GetActiveBugOwnersForFailedTests();
                if (MailConfiguration.To.IncludeActiveBugOwners)
                {
                    toAddressHashSet.AddRange(activeBugOwners);
                }
                else
                {
                    ccAddressHashSet.AddRange(activeBugOwners);
                }
            }


            if (MailConfiguration.To.IncludeChangesetOwners || MailConfiguration.Cc.IncludeChangesetOwners)
            {
                var associatedChanges = _reportData.AssociatedChanges;
                var changeSetOwners = GetChangesetOwners(associatedChanges);
                if (MailConfiguration.To.IncludeActiveBugOwners)
                {
                    toAddressHashSet.AddRange(changeSetOwners);
                }
                else
                {
                    ccAddressHashSet.AddRange(changeSetOwners);
                }
            }

            if (MailConfiguration.To.IncludeCreatedBy)
            {
                toAddressHashSet.Add(GetCreatedBy());
            }
            else if(MailConfiguration.Cc.IncludeCreatedBy)
            {
                ccAddressHashSet.Add(GetCreatedBy());
            }

            toAddressHashSet.AddRange(GetEmailAddressesFromString(MailConfiguration.To.DefaultRecipients));
            ccAddressHashSet.AddRange(GetEmailAddressesFromString(MailConfiguration.Cc.DefaultRecipients));

            recipientAdrresses.Add(RecipientType.TO, GetMailAddresses(toAddressHashSet)
                .DistinctBy(mailAddress => mailAddress.Address.ToLowerInvariant())
                .ToList());
            recipientAdrresses.Add(RecipientType.CC, GetMailAddresses(ccAddressHashSet)
                .DistinctBy(mailAddress => mailAddress.Address.ToLowerInvariant())
                .ToList());

            return recipientAdrresses;
        }

        private List<string> GetFailedTestOwners()
        {
            var mailAddresses = new List<string>();
            var failedTestOwners = _reportData.FailedTestOwners;
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

        private List<string> GetActiveBugOwnersForFailedTests()
        {
            var filteredTests = _reportData.FilteredResults;
            if (filteredTests == null)
            {
                return new List<string>();
            }

            List<string> bugOwners = filteredTests
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
            if (_reportData.CreatedBy == null)
            {
                throw new Exception("Unexpected error - CreatedBy found null");
            }

            return _reportData.CreatedBy.UniqueName;
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