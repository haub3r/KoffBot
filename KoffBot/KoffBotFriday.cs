using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Text;
using System;
using System.Data.SqlClient;
using System.Text.Json;
using System.Collections.Generic;
using System.Linq;

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
        using var httpClient = new HttpClient();

        var messages = Messages.FridayPossibilities;

        // Add season specific messages to message pool.
        if (DateTime.Now.Month >= 11 || DateTime.Now.Month <= 3)
        {
            messages = messages.Concat(Messages.FridayPossibilitiesWinter).ToArray();
        }

        if (DateTime.Now.Month == 6 
            || DateTime.Now.Month == 7
            || DateTime.Now.Month == 8
            || DateTime.Now.Month == 9)
        {
            messages = messages.Concat(Messages.FridayPossibilitiesSummer).ToArray();
        }

        Random random = new Random();
        int randomIndex = random.Next(0, messages.Length);
        var dto = new FridaySlackMessageDTO
        {
            Text = Messages.FridayPossibilities[randomIndex]
        };

        var content = new HttpRequestMessage(HttpMethod.Post, Shared.GetResponseEndpoint())
        {
            Content = new StringContent(JsonSerializer.Serialize(dto), Encoding.UTF8, "application/json")
        };
        await httpClient.SendAsync(content);

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
