using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Net;
using Belgrade.SqlClient.SqlDb;
using Belgrade.SqlClient;

namespace AzureFunctionSQL
{
    public static class Function1
    {
        [FunctionName("LatestEvent")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]HttpRequest req, ILogger log,
            ExecutionContext context)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            var config = new ConfigurationBuilder()
                 .SetBasePath(context.FunctionAppDirectory)
                 .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                 .AddEnvironmentVariables()
                 .Build();

            try {

                var ConnectionString = config.GetConnectionString("SqlConnection");

                var httpStatus = HttpStatusCode.OK;
                string body =
                    await (new QueryMapper(ConnectionString)
                                .OnError(ex => { httpStatus = HttpStatusCode.InternalServerError; }))
                    .GetString("EXECUTE [eventfeed].[dbo].[GetLatestSimpleEvent]");
                return new HttpResponseMessage() { Content = new StringContent(body), StatusCode = httpStatus };

            }
            catch (Exception ex)
            {
                log.LogError($"C# Http trigger function exception: {ex.Message}");
                return new HttpResponseMessage() { Content = new StringContent(""), StatusCode = HttpStatusCode.InternalServerError };
            }
        }
    }
}
