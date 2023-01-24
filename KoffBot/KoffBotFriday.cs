using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Text;
using System;
using System.Data.SqlClient;

namespace KoffBot;

public static class KoffBotFriday
{
    [FunctionName("KoffBotFriday")]
#if DEBUG
    public static async Task Run([TimerTrigger("0 */5 * * * *")] TimerInfo myTimer, ILogger log)
#else
    public static async Task Run([TimerTrigger("0 1 0 * * 5")] TimerInfo myTimer, ILogger log)
#endif
    {
        log.LogInformation("KoffBot activated. Ready to hail friday.");

        // Send message to Slack channel.
        var client = new HttpClient();
        Random random = new Random();
        int randomIndex = random.Next(0, Messages.FridayPossibilities.Length);
        var content = new HttpRequestMessage(HttpMethod.Post, Shared.GetResponseEndpoint())
        {
            Content = new StringContent("{\"text\": \"" + Messages.FridayPossibilities[randomIndex] + "\" }", Encoding.UTF8, "application/json")
        };
        await client.SendAsync(content);

        // Log the friday.
        try
        {
            var connectionString = Environment.GetEnvironmentVariable("DbConnectionString");
            using SqlConnection conn = new SqlConnection(connectionString);

            conn.Open();
            var sql = $@"INSERT INTO LogFriday (Created, CreatedBy, Modified, ModifiedBy)
                             VALUES (CURRENT_TIMESTAMP, 'KoffBotFriday', CURRENT_TIMESTAMP, 'KoffBotFriday')";

            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                var rows = await cmd.ExecuteNonQueryAsync();
            }
        }
        catch (Exception e)
        {
            log.LogError("Saving into friday log failed.", e);
        }
    }
}
