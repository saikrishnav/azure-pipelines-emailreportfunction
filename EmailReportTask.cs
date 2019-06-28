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

namespace EmailReportFunction
{
    public static class EmailReportTask
    {
        private static readonly EmailReport _emailReport;

        static EmailReportTask()
        {
            _emailReport = new EmailReport(new ReportFactory());
        }

        [FunctionName("EmailReportTask")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req, ILogger log)
        {
            log.LogInformation("EmailReportTask: HTTP trigger function started processing a request.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            string exMessage = null;
            bool status = false;
            try
            {
                status = await _emailReport.GenerateAndSendReport(requestBody, log);
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
