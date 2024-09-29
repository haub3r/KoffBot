using KoffBot.Database;
using KoffBot.Models;
using KoffBot.Messages;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;

namespace KoffBot;

public class KoffBotFridayFunction
{
    private readonly KoffBotContext _dbContext;
    private readonly ILogger _logger;

    public KoffBotFridayFunction(KoffBotContext dbContext, ILoggerFactory loggerFactory)
    {
        _dbContext = dbContext;
        _logger = loggerFactory.CreateLogger<KoffBotFridayFunction>();
    }

    [Function("KoffBotFriday")]
    public async Task Run([TimerTrigger("%TimerTriggerScheduleFridayFunction%")] TimerInfo myTimer)
    {
        _logger.LogInformation("KoffBot activated. Ready to hail friday.");

        var messages = FridayMessages.FridayPossibilities;

        // Add season specific messages to message pool.
        if (DateTime.Now.Month >= 11 || DateTime.Now.Month <= 3)
        {
            messages = [.. messages, .. FridayMessages.FridayPossibilitiesWinter];
        }

        if (DateTime.Now.Month == 6 
            || DateTime.Now.Month == 7
            || DateTime.Now.Month == 8
            || DateTime.Now.Month == 9)
        {
            messages = [.. messages, .. FridayMessages.FridayPossibilitiesSummer];
        }

        Random random = new();
        int randomIndex = random.Next(0, messages.Length);
        var dto = new FridaySlackMessage
        {
            Text = messages[randomIndex]
        };

        var content = new HttpRequestMessage(HttpMethod.Post, ResponseEndpointService.GetResponseEndpoint())
        {
            Content = new StringContent(JsonSerializer.Serialize(dto), Encoding.UTF8, "application/json")
        };

        // Send message to Slack channel.
        using var httpClient = new HttpClient();
        await httpClient.SendAsync(content);

        // Log the friday.
        try
        {
            var newRow = new LogFriday
            {
                Created = DateTime.Now,
                CreatedBy = "KoffBotFriday",
                Modified = DateTime.Now,
                ModifiedBy = "KoffBotFriday"
            };
            await _dbContext.AddAsync(newRow);
            await _dbContext.SaveChangesAsync();
        }
        catch (Exception e)
        {
            _logger.LogError("Saving into friday log failed. {e}", e);
        }
    }
}
