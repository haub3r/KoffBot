using KoffBot.Models;
using KoffBot.Models.Messages;
using KoffBot.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace KoffBot;

public class KoffBotAdvertisementFunction
{
    private readonly MessagingService _slackService;
    private readonly ILogger _logger;

    public KoffBotAdvertisementFunction(MessagingService slackService, ILoggerFactory loggerFactory)
    {
        _slackService = slackService;
        _logger = loggerFactory.CreateLogger<KoffBotAdvertisementFunction>();
    }

    // For this function to work, we would need to buy OpenAI API access again.
    [Function("KoffBotAdvertisement")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequestData req)
    {
        _logger.LogInformation("KoffBot activated. Ready to advertise using AI.");

        return await GetAiMessage(req);
    }

    private async Task<HttpResponseData> GetAiMessage(HttpRequestData req)
    {
        // Get message from OpenAI.
        using var httpClient = new HttpClient();
        string responseMessage;
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
                Content = new StringContent(JsonSerializer.Serialize(aiDto), Encoding.UTF8, new MediaTypeHeaderValue("application/json"))
            };
            aiRequest.Headers.Add("Authorization", Environment.GetEnvironmentVariable("OpenAiApiKey"));
            var response = await httpClient.SendAsync(aiRequest);
            var responseContent = await response.Content.ReadAsStringAsync();
            var parsed = JsonSerializer.Deserialize<AiResponse>(responseContent);
            responseMessage = parsed.Choices.First().Text;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Getting data from OpenAI failed.");
            var result = req.CreateResponse(HttpStatusCode.InternalServerError);
            result.WriteString("Getting data from OpenAI failed.");
            return result;
        }

        // Send message to Slack channel.
        var slackDto = new PriceSlackMessage
        {
            Text = responseMessage
        };
        await _slackService.PostMessageAsync(slackDto);

        return req.CreateResponse(HttpStatusCode.OK);
    }
}