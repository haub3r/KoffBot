using KoffBot.Models;
using KoffBot.Models.Logs;
using KoffBot.Models.Messages;
using KoffBot.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;

namespace KoffBot;

public class KoffBotToastFunction
{
    private readonly BlobStorageService _storageService;
    private readonly MessagingService _slackService;
    private readonly ILogger _logger;

    public KoffBotToastFunction(BlobStorageService storageService, MessagingService slackService, ILoggerFactory loggerFactory)
    {
        _storageService = storageService;
        _slackService = slackService;
        _logger = loggerFactory.CreateLogger<KoffBotToastFunction>();
    }

    [Function("KoffBotToast")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequestData req)
    {
        _logger.LogInformation("KoffBot activated. Ready for furious toasting.");
        var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", EnvironmentVariableTarget.Process);
        if (env != ResponseEndpointService.LocalEnvironmentName)
        {
            await AuthenticationService.Authenticate(req);
        }

        // Check if in "Drunk Mode".
        var drunkMode = false;
        try
        {
            var lastDrunk = await _storageService.GetLatestAsync<DefaultLog>(StorageContainers.LogDrunk);
            if (lastDrunk != null && DateTime.UtcNow < lastDrunk.Created.AddHours(1))
            {
                drunkMode = true;
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Reading from drunkedness log failed.");
            var result = req.CreateResponse(HttpStatusCode.InternalServerError);
            result.WriteString("Reading from drunkedness log failed.");

            return result;
        }

        // Send message to Slack channel.
        var message = new ToastSlackMessage()
        {
            Text = "Koff!"
        };

        if (drunkMode)
        {
            message.Text = ScrambleWord(message.Text);
        }

        await _slackService.PostMessageAsync(message);

        // Log the toasting.
        try
        {
            var newRow = new DefaultLog
            {
                Created = DateTime.UtcNow,
                CreatedBy = "KoffBotToast",
                Modified = DateTime.UtcNow,
                ModifiedBy = "KoffBotToast"
            };
            await _storageService.AddAsync(StorageContainers.LogToast, newRow);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Saving into toasting log failed.");
            var result = req.CreateResponse(HttpStatusCode.InternalServerError);
            result.WriteString("Saving into toasting log failed.");

            return result;
        }

        return req.CreateResponse(HttpStatusCode.OK);
    }

    private static string ScrambleWord(string str)
    {
        var rand = new Random();
        var list = new SortedList<int, char>();
        foreach (var c in str)
            list.Add(rand.Next(), c);

        return new string(list.Values.ToArray());
    }
}