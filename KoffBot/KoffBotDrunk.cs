using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Data.SqlClient;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace KoffBot;

public class KoffBotDrunk
{
    private readonly ILogger _logger;

    public KoffBotDrunk(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<KoffBotDrunk>();
    }

    [Function("KoffBotDrunk")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequestData req)
    {
        _logger.LogInformation("KoffBot activated. Ready to get rip-roaring drunk.");
        
        var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", EnvironmentVariableTarget.Process);
        if (env != Shared.LocalEnvironmentName)
        {
            await AuthenticationService.Authenticate(req, _logger);
        }

        // Send message to Slack channel.
        using var httpClient = new HttpClient();
        var dto = new DrunkSlackMessageDTO
        {
            Text = "KoffBot drank some delicious Koff beer and is now in 'Drunk Mode' for the next hour. Toasting will be difficult."
        };

        var content = new HttpRequestMessage(HttpMethod.Post, Shared.GetResponseEndpoint())
        {
            Content = new StringContent(JsonSerializer.Serialize(dto), Encoding.UTF8, "application/json")
        };
        await httpClient.SendAsync(content);

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
            _logger.LogError("Saving into drunkedness log failed.", e);
            var result = req.CreateResponse(HttpStatusCode.OK);
            result.WriteString("Saving into drunkedness log failed.");

            return result;
        }

        return req.CreateResponse(HttpStatusCode.OK);
    }
}
