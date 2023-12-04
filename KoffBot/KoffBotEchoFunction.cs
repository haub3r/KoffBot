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
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequestData req, FunctionContext functionContext)
    {
        _logger.LogInformation("KoffBot activated. Ready to echo some wise words.");

        var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", EnvironmentVariableTarget.Process);
        if (env != Shared.LocalEnvironmentName)
        {
            await AuthenticationService.Authenticate(req, _logger);
        }

        //_logger.LogInformation("Request from Slack: {requestBody}", JsonSerializer.Serialize(req));
        //_logger.LogInformation("Body from Slack: {requestBody}", JsonSerializer.Serialize(req.Body));
        _logger.LogInformation("Binding data from Slack: {requestBody}", JsonSerializer.Serialize(functionContext.BindingContext.BindingData));
        //var test = functionContext.BindingContext.BindingData.TryGetValue("ApplicationProperties", out var appProperties);
        //_logger.LogInformation("App properties from Slack: {requestBody}", JsonSerializer.Serialize(appProperties));
        //string requestBody32 = await new StreamReader(appProperties).ReadToEndAsync();
        string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        //_logger.LogInformation("Request body from Slack: {requestBody}", requestBody);
        NameValueCollection payload = HttpUtility.ParseQueryString(requestBody);
        //_logger.LogInformation("Payload from Slack: {payload}", payload);

        var userId = payload["user_id"];
        var userMessage = payload["text"];
        var userName = payload["user_name"];

        if (userId != IiroUserId || userId != IiroUserDevId)
        {
            _logger.LogWarning("Echo function was called by someone else than Iiro (it was {userName}). User ID of the caller: {userId}", userName, userId);
            return req.CreateResponse(HttpStatusCode.Forbidden);
        }

        using var httpClient = new HttpClient();
        var dto = new EchoSlackMessageDto
        {
            Text = userMessage
        };

        var content = new HttpRequestMessage(HttpMethod.Post, Shared.GetResponseEndpoint())
        {
            Content = new StringContent(JsonSerializer.Serialize(dto), Encoding.UTF8, "application/json")
        };
        await httpClient.SendAsync(content);

        return req.CreateResponse(HttpStatusCode.OK);
    }
}
