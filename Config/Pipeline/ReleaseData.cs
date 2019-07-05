using EmailReportFunction.Config.TestResults;
using EmailReportFunction.DataProviders;
using EmailReportFunction.Exceptions;
using Microsoft.VisualStudio.Services.ReleaseManagement.WebApi;
using Microsoft.VisualStudio.Services.WebApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmailReportFunction.Config.Pipeline
{
    public class ReleaseData : IPipelineData
    {
        public const string ReleaseEnvironmentIdString = "ReleaseEnvironmentId";
        public const string UsePrevReleaseEnvironmentString = "UsePrevReleaseEnvironment";

        public ReleaseData(Release release, 
            IReleaseDataProvider dataProvider, 
            IDataProvider<List<IdentityRef>> failedTestOwnersDataProvider,
            IDataProvider<FilteredTestResultData> testResultsDataProvider,
            IDataProvider<TestSummaryData> testSummaryDataProvider)
        {
            _release = release;
            _dataProvider = dataProvider;
            _failedTestOwnersDataProvider = failedTestOwnersDataProvider;
            _testResultsDataProvider = testResultsDataProvider;
            _testSummaryDataProvider = testSummaryDataProvider;
        }

        private Release _release;
        private IReleaseDataProvider _dataProvider;
        private IDataProvider<List<IdentityRef>> _failedTestOwnersDataProvider;
        private IDataProvider<FilteredTestResultData> _testResultsDataProvider;
        private IDataProvider<TestSummaryData> _testSummaryDataProvider;
        private ReleaseEnvironment _releaseEnvironment;

        private Release _lastCompletedRelease;

        #region Public methods 

        public Release Release => this._release;

        public IdentityRef CreatedBy => _release.CreatedBy;


        public ReleaseEnvironment Environment
        {
            get
            {
                if(_releaseEnvironment == null)
                {
                    _releaseEnvironment = GetEnvironment();
                }
                return _releaseEnvironment;
            }
        }

        public async Task<Release> GetLastCompletedReleaseAsync()
        {
            if (_lastCompletedRelease == null)
            {
                _lastCompletedRelease = await _dataProvider.GetReleaseByLastCompletedEnvironmentAsync(_release, this.Environment);
            }
            return _lastCompletedRelease;
        }

        public async Task<ReleaseEnvironment> GetLastCompletedEnvironmentAsync()
        {
            await GetLastCompletedReleaseAsync();
            return _lastCompletedRelease?.Environments?.FirstOrDefault(e => e.DefinitionEnvironmentId == this.Environment.DefinitionEnvironmentId);
        }

        public async Task<List<ChangeData>> GetAssociatedChangesAsync()
        {
            await GetLastCompletedReleaseAsync();
            return await _dataProvider.GetAssociatedChangesAsync(_lastCompletedRelease);
        }

        public async Task<List<PhaseData>> GetPhasesAsync()
        {
            return await _dataProvider.GetPhasesAsync(this.Environment);
        }

        public async Task<List<IdentityRef>> GetFailedTestOwnersAsync()
        {
            return await _failedTestOwnersDataProvider.GetDataAsync();
        }

        public async Task<FilteredTestResultData> GetFilteredTestsAsync()
        {
            return await _testResultsDataProvider.GetDataAsync();
        }

        public async Task<TestSummaryData> GetTestSummaryDataAsync()
        {
            return await _testSummaryDataProvider.GetDataAsync();
        }

        #endregion

        #region Helpers 

        private int ReleaseEnvironmentId
        {
            get
            {
                int envId = -1;
                this._release.Properties.TryGetValue(ReleaseEnvironmentIdString, out envId);
                return envId;
            }
        }

        private bool UsePreviousEnvironment
        {
            get
            {
                bool usePrevEnv = false;
                this._release.Properties.TryGetValue(UsePrevReleaseEnvironmentString, out usePrevEnv);
                return usePrevEnv;
            }
        }

        private ReleaseEnvironment GetEnvironment()
        {
            ReleaseEnvironment environment = null;
            if (this.ReleaseEnvironmentId > 0)
            {
                environment = this._release.Environments.FirstOrDefault(env => env.Id == ReleaseEnvironmentId);

                if (this.UsePreviousEnvironment)
                {
                    if (this._release.Environments.IndexOf(environment) - 1 < 0)
                    {
                        throw new ReleaseDataException(
                        $"Unable to find previous environment for given environment id - {ReleaseEnvironmentId} in release - {this._release.Id}");
                    }
                    environment = this._release.Environments[this._release.Environments.IndexOf(environment) - 1];
                }
            }

            if (environment == null)
            {
                throw new ReleaseDataException(
                    $"Unable to find environment with environment id - {ReleaseEnvironmentId} in release - {this._release.Id}");
            }

            return environment;
        }

        #endregion

        public override string ToString()
        {
            return $"ReleaseId: {this._release.Id}, EnvironmentId: {this.ReleaseEnvironmentId}";
        }
    }
}
