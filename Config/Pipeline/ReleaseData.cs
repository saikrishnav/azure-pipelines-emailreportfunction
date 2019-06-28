using EmailReportFunction.Config.TestResults;
using EmailReportFunction.DataProviders;
using EmailReportFunction.Exceptions;
using Microsoft.TeamFoundation.TestManagement.WebApi;
using Microsoft.VisualStudio.Services.ReleaseManagement.WebApi;
using Microsoft.VisualStudio.Services.ReleaseManagement.WebApi.Contracts;
using Microsoft.VisualStudio.Services.WebApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace EmailReportFunction.Config.Pipeline
{
    public class ReleaseData : IPipelineData
    {
        public const string ReleaseEnvironmentIdString = "ReleaseEnvironmentId";
        public const string UsePrevReleaseEnvironmentString = "UsePrevReleaseEnvironment";

        public ReleaseData(Release release, IReleaseDataProvider dataProvider)
        {
            _release = release;
            _dataProvider = dataProvider;
        }

        private Release _release;
        private IReleaseDataProvider _dataProvider;
        private ReleaseEnvironment _releaseEnvironment;

        private Release _lastCompletedRelease;

        #region Public methods 

        public IList<Artifact> Artifacts => this._release.Artifacts;

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
                _lastCompletedRelease = await _dataProvider.GetReleaseByLastCompletedEnvironment(_release, this.Environment);
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
            return await _dataProvider.GetAssociatedChanges(_lastCompletedRelease);
        }

        public async Task<List<PhaseData>> GetPhasesAsync()
        {
            return await _dataProvider.GetPhases(this.Environment);
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
