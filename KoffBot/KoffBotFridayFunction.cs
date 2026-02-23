using KoffBot.Messages;
using KoffBot.Models;
using KoffBot.Models.Logs;
using KoffBot.Models.Messages;
using KoffBot.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace KoffBot;

public class KoffBotFridayFunction
{
    private readonly BlobStorageService _storageService;
    private readonly MessagingService _slackService;
    private readonly ILogger _logger;

    public KoffBotFridayFunction(BlobStorageService storageService, MessagingService slackService, ILoggerFactory loggerFactory)
    {
        _storageService = storageService;
        _slackService = slackService;
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

        // Send message to Slack channel.
        await _slackService.PostMessageAsync(dto);

        // Log the friday.
        try
        {
            var newRow = new DefaultLog
            {
                Created = DateTime.UtcNow,
                CreatedBy = "KoffBotFriday",
                Modified = DateTime.UtcNow,
                ModifiedBy = "KoffBotFriday"
            };
            await _storageService.AddAsync(StorageContainers.LogFriday, newRow);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Saving into friday log failed.");
        }
    }
}
