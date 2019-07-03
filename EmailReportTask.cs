using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using EmailReportFunction.DataProviders;
using System.Runtime.CompilerServices;
using EmailReportFunction.Config;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.KeyVault.Models;
using Newtonsoft.Json.Linq;
using EmailReportFunction.Wrappers;
using EmailReportFunction.Utils;

namespace EmailReportFunction
{
    public static class EmailReportTask
    {
        [FunctionName("EmailReportTask")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req, ILogger logger)
        {
            logger.LogInformation("EmailReportTask: HTTP trigger function started processing a request.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var emailReportConfig = RequestHelper.CreateConfiguration(requestBody, logger);

            var reportFactory = new ReportFactory(emailReportConfig, logger);

            string exMessage = null;
            bool status = false;
            try
            {
                status = await new EmailReport(reportFactory).GenerateAndSendReport(emailReportConfig);
            }
            catch(Exception ex)
            {
                exMessage = ex.Message;
            }

            var returnStr = status ? $"Hello, World. Mail Sent." : exMessage;
            return (ActionResult)new OkObjectResult(returnStr);
                //: new BadRequestObjectResult("Please pass a name on the query string or in the request body");
        }
    }
}
