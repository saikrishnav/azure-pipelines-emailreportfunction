using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using EmailReportFunction.Wrappers;
using EmailReportFunction.Utils;

namespace EmailReportFunction
{
    public static class EmailReportTask
    {
        [FunctionName("EmailReportTask")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req, ILogger logger, ExecutionContext executionContext)
        {
            logger.LogInformation("EmailReportTask: HTTP trigger function started processing a request.");

            string resultStr = string.Empty;
            if (req.Method == HttpMethods.Get)
            {
                string jsonFilePath = Path.Combine(executionContext.FunctionAppDirectory, "sampleJsonRequest.json");
                resultStr = "Here's a Sample JSON request:\r\n" + File.ReadAllText(jsonFilePath);
            }
            else if (req.Method == HttpMethods.Post)
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var emailReportConfig = RequestHelper.CreateConfiguration(requestBody, logger);

                var reportFactory = new ReportFactory(emailReportConfig, logger);

                string exMessage = null;
                bool status = false;
                try
                {
                    status = await new EmailReport(reportFactory).GenerateAndSendReport(emailReportConfig);
                }
                catch (Exception ex)
                {
                    exMessage = ex.Message;
                }

                resultStr = status ? $"Mail Sent Successfully." : exMessage + "\r\n. Request not processed properly: \r\n" + requestBody;
            }

            return new OkObjectResult(resultStr);
                //: new BadRequestObjectResult("Please pass a name on the query string or in the request body");
        }
    }
}
