using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace KoffBot;

public class KoffBotUntappd
{
    private readonly ILogger _logger;

    public KoffBotUntappd(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<KoffBotUntappd>();
    }

    [Function("KoffBotUntappd")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequestData req,
        ILogger logger)
    {
        logger.LogInformation("KoffBot activated. Ready to advertise untappd.");

        var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", EnvironmentVariableTarget.Process);
        if (env != Shared.LocalEnvironmentName)
        {
            await AuthenticationService.Authenticate(req, logger);
        }

        using var httpClient = new HttpClient();
        var dto = new UntappdSlackMessageDTO
        {
            Text = $"Muista, että voit arvostella Koffin Untappd-sovelluksessa. Sovelluksen saa osoitteesta: www.untappd.com. Annathan Koffille viisi tähteä :koff:"
        };

        var content = new HttpRequestMessage(HttpMethod.Post, Shared.GetResponseEndpoint())
        {
            Content = new StringContent(JsonSerializer.Serialize(dto), Encoding.UTF8, "application/json")
        };
        await httpClient.SendAsync(content);

        return req.CreateResponse(HttpStatusCode.OK);
    }
}
