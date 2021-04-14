using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Text;
using System;
using System.Data.SqlClient;

namespace KoffBot
{
    public static class KoffBotToast
    {
        [FunctionName("KoffBotToast")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("KoffBot activated. Ready for furious toasting.");
#if !DEBUG
            await AuthenticationService.Authenticate(req, log);
#endif
            // Send message to Slack channel.
            var client = new HttpClient();
            var content = new HttpRequestMessage(HttpMethod.Post, Shared.GetResponseEndpoint())
            {
                Content = new StringContent("{\"text\": \"Koff!\" }", Encoding.UTF8, "application/json")
            };
            await client.SendAsync(content);

            // Log the toasting.
            try
            {
                var connectionString = Environment.GetEnvironmentVariable("DbConnectionString");
                using SqlConnection conn = new SqlConnection(connectionString);

                conn.Open();
                var sql = $@"INSERT INTO LogToast (Created, CreatedBy, Modified, ModifiedBy)
                             VALUES (CURRENT_TIMESTAMP, 'KoffBotToast', CURRENT_TIMESTAMP, 'KoffBotToast')";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    await cmd.ExecuteNonQueryAsync();
                }
            }
            catch (Exception e)
            {
                log.LogError("Saving into toasting log failed.", e);
                var result = new ObjectResult("Saving into toasting log failed.")
                {
                    StatusCode = StatusCodes.Status500InternalServerError
                };
                return result;
            }

            return new OkResult();
        }
    }
}
