using KoffBot.Dtos;
using KoffBot.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Collections.Specialized;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Web;

namespace KoffBot;

public class KoffBotEchoFunction
{
    private readonly ILogger _logger;
    private const string IiroUserId = "U0106R0N6NB";
    private const string IiroUserDevId = "U01D73HKFC3";

    public KoffBotEchoFunction(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<KoffBotEchoFunction>();
    }

    [Function("KoffBotEcho")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequestData req)
    {
        _logger.LogInformation("KoffBot activated. Ready to echo some wise words.");

        var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", EnvironmentVariableTarget.Process);
        if (env != ResponseEndpointService.LocalEnvironmentName)
        {
            await AuthenticationService.Authenticate(req);
        }

        // Authentication reads the stream also, so we need to set the position to 0 manually.
        req.Body.Position = 0;
        string requestBody = await req.ReadAsStringAsync();
        NameValueCollection payload = HttpUtility.ParseQueryString(requestBody);

        var userId = payload["user_id"];
        var userMessage = payload["text"];
        var userName = payload["user_name"];

        if (userId != IiroUserId && userId != IiroUserDevId)
        {
            _logger.LogWarning("Echo function was called by someone else than Iiro (it was {userName}). User ID of the caller: {userId}", userName, userId);
            return req.CreateResponse(HttpStatusCode.Forbidden);
        }

        using var httpClient = new HttpClient();
        var dto = new EchoSlackMessageDto
        {
            Text = userMessage
        };

        var content = new HttpRequestMessage(HttpMethod.Post, ResponseEndpointService.GetResponseEndpoint())
        {
            Content = new StringContent(JsonSerializer.Serialize(dto), Encoding.UTF8, "application/json")
        };
        await httpClient.SendAsync(content);

        return req.CreateResponse(HttpStatusCode.OK);
    }
}
