using EmailReportFunction.Config;
using EmailReportFunction.Config.Pipeline;
using EmailReportFunction.DataProviders;
using EmailReportFunction.Exceptions;
using EmailReportFunction.Utils;
using EmailReportFunction.Wrappers;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.Services.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EmailReportFunction.Report
{
    public class ReportGenerator : IReportGenerator
    {
        private readonly IEnumerable<IDataProvider> _dataProviders;

        private readonly IEnumerable<IDataProvider> _dataPostProcessors;

        private readonly ILogger _logger;

        private readonly PipelineType _pipelineType;

        public ReportGenerator(IDataProviderFactory dataProviderFactory, PipelineType pipelineType, ILogger logger)
        {
            _logger = logger;
            _dataProviders = dataProviderFactory.GetPipelineDataProviders();
            _dataPostProcessors = dataProviderFactory.GetPostProcessors();
            _pipelineType = pipelineType;
        }

        public async Task<AbstractReport> FetchReportAsync()
        {
            var reportData = _pipelineType == PipelineType.Release ? new ReleaseReport() : null;
            using (new PerformanceMeasurementBlock("Get report data from All Providers", _logger))
            {
                try
                {
                    var reports = await _dataProviders.ParallelSelectAsync(async provider =>
                    {
                        var newReport = reportData.CreateEmptyReportData();
                        try
                        {
                            await provider.AddReportDataAsync(newReport);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError($"Exception thrown in {provider.GetType()} - {ex}");
                            reportData.DataMissing = true;
                        }

                        return newReport;
                    });

                    // Merge dtos obtained from all the providers
                    reports.ForEach(report => reportData.Merge(report));

                    // PostProcessors
                    _dataPostProcessors.ForEach(processor => processor.AddReportDataAsync(reportData));
                }
                catch (EmailReportException e)
                {
                    _logger.LogError(e.Message);
                    reportData.DataMissing = true;
                }
            }

            return reportData;
        }
    }
}
