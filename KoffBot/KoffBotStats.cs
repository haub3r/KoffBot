using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Data.SqlClient;
using System.Net;
using System.Threading.Tasks;

namespace KoffBot;

public class KoffBotStats
{
    private readonly ILogger _logger;

    public KoffBotStats(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<KoffBotStats>();
    }

    [Function("KoffBotStats")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequestData req)
    {
        _logger.LogInformation("KoffBot activated. Ready to retrieve epic stats.");

        // Get the stats.
        try
        {
            var result = new StatsDTO();
            var connectionString = Environment.GetEnvironmentVariable("DbConnectionString");
            using SqlConnection conn = new SqlConnection(connectionString);

            conn.Open();
            var sql = $@"SELECT (SELECT COUNT(0) FROM LogToast) as ToastCount, 
                                    (SELECT COUNT(0) FROM LogFriday) as FridayCount,
                                    (SELECT COUNT(0) FROM LogDrunk) as DrunkCount";

            using SqlCommand cmd = new SqlCommand(sql, conn);
            var rows = await cmd.ExecuteReaderAsync();
            while (await rows.ReadAsync())
            {
                result.ToastCount = Convert.ToInt32(rows[0]);
                result.FridayCount = Convert.ToInt32(rows[1]);
                result.DrunkCount = Convert.ToInt32(rows[2]);
            }

            rows.Close();

            return req.CreateResponse(HttpStatusCode.OK);
        }
        catch (Exception e)
        {
            _logger.LogError("Getting the stats failed.", e);
            throw;
        }
    }
}
