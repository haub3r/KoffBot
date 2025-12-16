using KoffBot.Messages;
using KoffBot.Models;
using KoffBot.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;

namespace KoffBot;

public class KoffBotFridayFunction
{
    private readonly BlobStorageService _storageService;
    private readonly ILogger _logger;

    public KoffBotFridayFunction(BlobStorageService storageService, ILoggerFactory loggerFactory)
    {
        _storageService = storageService;
        _logger = loggerFactory.CreateLogger<KoffBotFridayFunction>();
    }

    [Function("KoffBotFriday")]
    public async Task Run([TimerTrigger("%TimerTriggerScheduleFridayFunction%")] TimerInfo myTimer)
    {
        _logger.LogInformation("KoffBot activated. Ready to hail friday.");

        string[] messages = [];

        // Check for friday the 13.
        if (DateTime.Now.Day == 13)
        {
            messages = FridayMessages.Friday13Possibilities;
        }
        else
        {
            messages = FridayMessages.NormalFridayPossibilities;

            // Add season specific messages to message pool.
            if (DateTime.Now.Month >= 11 || DateTime.Now.Month <= 3)
            {
                messages = [.. messages, .. FridayMessages.NormalFridayPossibilitiesWinter];
            }

            if (DateTime.Now.Month == 6
                || DateTime.Now.Month == 7
                || DateTime.Now.Month == 8
                || DateTime.Now.Month == 9)
            {
                messages = [.. messages, .. FridayMessages.NormalFridayPossibilitiesSummer];
            }
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
            var newRow = new DefaultLog
            {
                Created = DateTime.Now,
                CreatedBy = "KoffBotFriday",
                Modified = DateTime.Now,
                ModifiedBy = "KoffBotFriday"
            };
            await _storageService.AddAsync(StorageContainers.LogFriday, newRow);
        }
        catch (Exception e)
        {
            _logger.LogError("Saving into friday log failed. {e}", e);
        }
    }
}
