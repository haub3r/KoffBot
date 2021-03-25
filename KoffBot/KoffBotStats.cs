using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Data.SqlClient;

namespace KoffBot
{
    public static class KoffBotStats
    {
        [FunctionName("KoffBotStats")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("KoffBot activated. Ready to retrieve epic stats.");

            // Get the stats.
            try
            {
                var result = new StatsDTO();
                var connectionString = Environment.GetEnvironmentVariable("DbConnectionString");
                using SqlConnection conn = new SqlConnection(connectionString);

                conn.Open();
                var sql = $@"SELECT COUNT(0) as ToastCount, (SELECT COUNT(0) FROM LogFriday) as FridayCount FROM LogToast";

                using SqlCommand cmd = new SqlCommand(sql, conn);
                var rows = await cmd.ExecuteReaderAsync();
                while (await rows.ReadAsync())
                {
                    result.ToastCount = Convert.ToInt32(rows[0]);
                    result.FridayCount = Convert.ToInt32(rows[1]);
                }

                rows.Close();

                return new OkObjectResult(result);
            }
            catch (Exception e)
            {
                log.LogError("Getting the stats failed.", e);
                var result = new ObjectResult("Getting the stats failed.")
                {
                    StatusCode = StatusCodes.Status500InternalServerError
                };
                return result;
            }
        }
    }
}
