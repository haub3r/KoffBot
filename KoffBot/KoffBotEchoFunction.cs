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

    public KoffBotEchoFunction(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<KoffBotEchoFunction>();
    }

    [Function("KoffBotEcho")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequestData req)
    {
        _logger.LogInformation("KoffBot activated. Ready to echo some wise words.");

        var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", EnvironmentVariableTarget.Process);
        if (env != Shared.LocalEnvironmentName)
        {
            await AuthenticationService.Authenticate(req, _logger);
        }

        string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        NameValueCollection payload = HttpUtility.ParseQueryString(requestBody);
        _logger.LogInformation($"Payload from Slack: {payload}");

        if (payload["user_id"] != IiroUserId)
        {
            _logger.LogWarning($"Echo function was called by someone else than Iiro (it was {payload["user_name"]}). User ID of the caller: {payload["user_id"]}");
            return req.CreateResponse(HttpStatusCode.Forbidden);
        }

        using var httpClient = new HttpClient();
        var dto = new EchoSlackMessageDto
        {
            Text = payload["text"]
        };

        var content = new HttpRequestMessage(HttpMethod.Post, Shared.GetResponseEndpoint())
        {
            Content = new StringContent(JsonSerializer.Serialize(dto), Encoding.UTF8, "application/json")
        };
        await httpClient.SendAsync(content);

        return req.CreateResponse(HttpStatusCode.OK);
    }
}
