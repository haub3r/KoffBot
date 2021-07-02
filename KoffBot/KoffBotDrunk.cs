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
    public static class KoffBotDrunk
    {
        [FunctionName("KoffBotDrunk")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("KoffBot activated. Ready to get rip-roaring drunk.");
#if !DEBUG
            await AuthenticationService.Authenticate(req, log);
#endif
            // Send message to Slack channel.
            var client = new HttpClient();
            var content = new HttpRequestMessage(HttpMethod.Post, Shared.GetResponseEndpoint())
            {
                Content = new StringContent("{\"text\": \"KoffBot drank some delicious Koff beer and is now on 'Drunk Mode' for the next hour. Toasting will be difficult.\" }", Encoding.UTF8, "application/json")
            };
            await client.SendAsync(content);

            // Log the drunkedness.
            try
            {
                var connectionString = Environment.GetEnvironmentVariable("DbConnectionString");
                using SqlConnection conn = new SqlConnection(connectionString);

                conn.Open();
                var sql = $@"INSERT INTO LogDrunk (Created, CreatedBy, Modified, ModifiedBy)
                             VALUES (CURRENT_TIMESTAMP, 'KoffBotDrunk', CURRENT_TIMESTAMP, 'KoffBotDrunk')";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    await cmd.ExecuteNonQueryAsync();
                }
            }
            catch (Exception e)
            {
                log.LogError("Saving into drunkedness log failed.", e);
                var result = new ObjectResult("Saving into drunkedness log failed.")
                {
                    StatusCode = StatusCodes.Status500InternalServerError
                };
                return result;
            }

            return new OkResult();
        }
    }
}
