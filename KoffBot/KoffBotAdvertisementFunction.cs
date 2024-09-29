using KoffBot.Models;
using KoffBot.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text;
using System.Text.Json;

namespace KoffBot;

public class KoffBotAdvertisementFunction
{
    private readonly ILogger _logger;

    public KoffBotAdvertisementFunction(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<KoffBotAdvertisementFunction>();
    }

    // For this function to work, we would need to buy OpenAI API access again.
    [Function("KoffBotAdvertisement")]
    public async Task Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequestData req)
    {
        _logger.LogInformation("KoffBot activated. Ready to advertise using AI.");

        var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", EnvironmentVariableTarget.Process);
        if (env != ResponseEndpointService.LocalEnvironmentName)
        {
            await AuthenticationService.Authenticate(req);
        }

        await GetAiMessage(_logger, req);
    }

    private static async Task GetAiMessage(ILogger logger, HttpRequestData req)
    {
        // Get message from OpenAI.
        using var httpClient = new HttpClient();
        string responseMessage = "";
        try
        {
            var aiDto = new AiRequest
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
            var parsed = JsonSerializer.Deserialize<AiResponse>(responseContent);
            responseMessage = parsed.Choices.First().Text;
        }
        catch (Exception e)
        {
            logger.LogError("Getting data from OpenAI failed. {e}", e);
            var result = req.CreateResponse(HttpStatusCode.InternalServerError);
        }

        // Send message to Slack channel.
        var slackDto = new PriceSlackMessage
        {
            Text = responseMessage
        };

        var slackRequest = new HttpRequestMessage(HttpMethod.Post, ResponseEndpointService.GetResponseEndpoint())
        {
            Content = new StringContent(JsonSerializer.Serialize(slackDto), Encoding.UTF8, "application/json")
        };
        await httpClient.SendAsync(slackRequest);
    }
}