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
using System.Text.Json;
using System.Collections.Generic;
using System.Linq;

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
            // Check if in "Drunk Mode".
            var drunkMode = false;
            try
            {
                var connectionString = Environment.GetEnvironmentVariable("DbConnectionString");
                using SqlConnection conn = new SqlConnection(connectionString);

                conn.Open();
                var sqlGet = $@"SELECT TOP 1 * FROM LogDrunk ORDER BY id DESC";

                using SqlCommand cmd = new SqlCommand(sqlGet, conn);
                var rows = await cmd.ExecuteReaderAsync();
                var lastDate = new DateTime();
                while (await rows.ReadAsync())
                {
                    lastDate = Convert.ToDateTime(rows[4]);
                }

                rows.Close();

                if (DateTime.UtcNow < lastDate.AddHours(1))
                {
                    drunkMode = true;
                }
            }
            catch (Exception e)
            {
                log.LogError("Reading from drunkedness log failed.", e);
                var result = new ObjectResult("Reading from drunkedness log failed.")
                {
                    StatusCode = StatusCodes.Status500InternalServerError
                };
                return result;
            }

            // Send message to Slack channel.
            var message = new SlackMessageDTO()
            {
                text = "Koff!"
            };

            if (drunkMode)
            {
                message.text = ScrambleWord(message.text);
            }

            var client = new HttpClient();
            var content = new HttpRequestMessage(HttpMethod.Post, Shared.GetResponseEndpoint())
            {
                Content = new StringContent(JsonSerializer.Serialize(message), Encoding.UTF8, "application/json")
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

        private static string ScrambleWord(string str)
        {
            var rand = new Random(); var list = new SortedList<int, char>();
            foreach (var c in str)
                list.Add(rand.Next(), c);
            
            return new string(list.Values.ToArray());
        }
    }
}