using KoffBot.Models.Messages;
using KoffBot.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;

namespace KoffBot;

public class KoffBotUntappdFunction
{
    private readonly MessagingService _slackService;
    private readonly ILogger _logger;

    public KoffBotUntappdFunction(MessagingService slackService, ILoggerFactory loggerFactory)
    {
        _slackService = slackService;
        _logger = loggerFactory.CreateLogger<KoffBotUntappdFunction>();
    }

    [Function("KoffBotUntappd")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequestData req)
    {
        _logger.LogInformation("KoffBot activated. Ready to advertise Untappd.");

        var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", EnvironmentVariableTarget.Process);
        if (env != ResponseEndpointService.LocalEnvironmentName)
        {
            await AuthenticationService.Authenticate(req);
        }

        var dto = new UntappdSlackMessage
        {
            Text = "Muista, että voit arvostella Koffin Untappd-sovelluksessa. Sovelluksen saa osoitteesta: www.untappd.com. Annathan Koffille viisi tähteä :koff:"
        };
        await _slackService.PostMessageAsync(dto);

        return req.CreateResponse(HttpStatusCode.OK);
    }
}
