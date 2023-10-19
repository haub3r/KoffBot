using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace KoffBot;

public class KoffBotFridayFunction
{
    private readonly ILogger _logger;

    public KoffBotFridayFunction(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<KoffBotFridayFunction>();
    }

    [Function("KoffBotFriday")]
#if DEBUG
    public async Task Run([TimerTrigger("0 */5 * * * *")] TimerInfo myTimer)
#else
    public async Task Run([TimerTrigger("0 1 0 * * 5")] TimerInfo myTimer)
#endif
    {
        _logger.LogInformation("KoffBot activated. Ready to hail friday.");

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
            Text = messages[randomIndex]
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
            _logger.LogError("Saving into friday log failed.", e);
        }
    }
}
