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

namespace KoffBot;

public static class KoffBotDrunk
{
    [FunctionName("KoffBotDrunk")]
    public static async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
        ILogger logger)
    {
        logger.LogInformation("KoffBot activated. Ready to get rip-roaring drunk.");
        
        var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", EnvironmentVariableTarget.Process);
        if (env != Shared.LocalEnvironmentName)
        {
            await AuthenticationService.Authenticate(req, logger);
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
            logger.LogError("Saving into drunkedness log failed.", e);
            var result = new ObjectResult("Saving into drunkedness log failed.")
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };
            return result;
        }

        return new OkResult();
    }
}
