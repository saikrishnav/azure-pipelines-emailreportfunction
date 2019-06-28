using Microsoft.VisualStudio.Services.WebApi;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EmailReportFunction.Config.Pipeline
{
    public interface IPipelineData
    {
        Task<List<ChangeData>> GetAssociatedChangesAsync();

        Task<List<IdentityRef>> GetFailedTestOwnersAsync();
    }
}
