using KoffBot.Models;
using KoffBot.Models.Logs;
using KoffBot.Models.Messages;
using KoffBot.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;

namespace KoffBot;

public class KoffBotDrunkFunction
{
    private readonly BlobStorageService _storageService;
    private readonly MessagingService _slackService;
    private readonly ILogger _logger;

    public KoffBotDrunkFunction(BlobStorageService storageService, MessagingService slackService, ILoggerFactory loggerFactory)
    {
        _storageService = storageService;
        _slackService = slackService;
        _logger = loggerFactory.CreateLogger<KoffBotDrunkFunction>();
    }

    [Function("KoffBotDrunk")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequestData req)
    {
        _logger.LogInformation("KoffBot activated. Ready to get rip-roaring drunk.");

        // Send message to Slack channel.
        var dto = new DrunkSlackMessage
        {
            Text = "KoffBot drank some delicious Koff beer and is now in 'Drunk Mode' for the next hour. Toasting will be difficult."
        };
        await _slackService.PostMessageAsync(dto);

        // Log the drunkedness.
        try
        {
            var newRow = new DefaultLog
            {
                Created = DateTime.UtcNow,
                CreatedBy = "KoffBotDrunk",
                Modified = DateTime.UtcNow,
                ModifiedBy = "KoffBotDrunk"
            };
            await _storageService.AddAsync(StorageContainers.LogDrunk, newRow);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Saving into drunkedness log failed.");
            var result = req.CreateResponse(HttpStatusCode.OK);
            result.WriteString("Saving into drunkedness log failed.");

            return result;
        }

        return req.CreateResponse(HttpStatusCode.OK);
    }
}
