using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using EmailReportFunction.Utils;
using EmailReportFunction.Report;

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
                var emailReportConfig = RequestHelper.CreateConfiguration(req.Headers, requestBody, logger);

                if (emailReportConfig == null)
                {
                    resultStr = "ExecuteCondition evaluation didn't pass. Not generating report.";
                }
                else
                {
                    var reportFactory = new ReportFactory(emailReportConfig, logger);
                    try
                    {
                        // TODO: Azure Pipelines Serverless Tasks do not support tasks that take >20s. 
                        new EmailReport(reportFactory).GenerateAndSendReport();
                        //resultStr = status ? "Mail Sent Successfully" : "Mail Not Sent";
                        resultStr = "Request Processing Started successfully. Mail will be sent based on SendMailCondition evaluation.";
                    }
                    catch (Exception ex)
                    {
                        resultStr = $"Request not processed properly: \r\n{ex.Message} \r\n\r\nRequestBody for reference: {requestBody}";
                    }
                }
            }

            return new OkObjectResult(resultStr);
                //: new BadRequestObjectResult("Please pass a name on the query string or in the request body");
        }
    }
}
