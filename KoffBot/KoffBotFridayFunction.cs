using KoffBot.Database;
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
            messages = [.. messages, .. Messages.FridayPossibilitiesWinter];
        }

        if (DateTime.Now.Month == 6 
            || DateTime.Now.Month == 7
            || DateTime.Now.Month == 8
            || DateTime.Now.Month == 9)
        {
            messages = [.. messages, .. Messages.FridayPossibilitiesSummer];
        }

        Random random = new();
        int randomIndex = random.Next(0, messages.Length);
        var dto = new FridaySlackMessageDto
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
            var newRow = new LogFriday
            {
                Created = DateTime.Now,
                CreatedBy = "KoffBotFriday",
                Modified = DateTime.Now,
                ModifiedBy = "KoffBotFriday"
            };
            _dbContext.Add(newRow);
            _dbContext.SaveChanges();
        }
        catch (Exception e)
        {
            _logger.LogError("Saving into friday log failed. {e}", e);
        }
    }
}
