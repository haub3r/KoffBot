using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace KoffBot;

public class KoffBotAdvertisementFunction
{
    private readonly ILogger _logger;

    public KoffBotAdvertisementFunction(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<KoffBotAdvertisementFunction>();
    }

    [Function("KoffBotAdvertisement")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequestData req)
    {
        _logger.LogInformation("KoffBot activated. Ready to advertise using AI.");

        var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", EnvironmentVariableTarget.Process);
        if (env != Shared.LocalEnvironmentName)
        {
            await AuthenticationService.Authenticate(req, _logger);
        }

        // Run without awaiting to avoid Slack errors to users.
        Task<HttpResponseData> task = Task.Run(() => GetAiMessage(_logger, req));
        return req.CreateResponse(HttpStatusCode.OK);
    }

    private static async Task<HttpResponseData> GetAiMessage(ILogger logger, HttpRequestData req)
    {
        // Get message from OpenAI.
        using var httpClient = new HttpClient();
        string responseMessage;
        try
        {
            var aiDto = new AiRequestDto
            {
                MaxTokens = 150,
                Model = "text-davinci-002",
                Prompt = "Write an advertisement about beer that is named Koff in Finnish language.",
                Temperature = 0.6
            };
            var aiRequest = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/completions")
            {
                Content = new StringContent(JsonSerializer.Serialize(aiDto), Encoding.UTF8, "application/json")
            };
            aiRequest.Headers.Add("Authorization", Environment.GetEnvironmentVariable("OpenAiApiKey"));
            var response = await httpClient.SendAsync(aiRequest);
            var responseContent = await response.Content.ReadAsStringAsync();
            var parsed = JsonSerializer.Deserialize<AiResponseDto>(responseContent);
            responseMessage = parsed.Choices.First().Text;
        }
        catch (Exception e)
        {
            logger.LogError("Getting data from OpenAI failed.", e);
            var result = req.CreateResponse(HttpStatusCode.InternalServerError);
            result.WriteString("Getting data from OpenAI failed.");
            return result;
        }

        // Send message to Slack channel.
        var slackDto = new PriceSlackMessageDto
        {
            Text = responseMessage
        };

        var slackRequest = new HttpRequestMessage(HttpMethod.Post, Shared.GetResponseEndpoint())
        {
            Content = new StringContent(JsonSerializer.Serialize(slackDto), Encoding.UTF8, "application/json")
        };
        await httpClient.SendAsync(slackRequest);

        return req.CreateResponse(HttpStatusCode.OK);
    }
}