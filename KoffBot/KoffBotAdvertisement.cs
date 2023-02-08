using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Text;
using System;
using System.Text.Json;
using System.Linq;

namespace KoffBot;

public static class KoffBotAdvertisement
{
    [FunctionName("KoffBotAdvertisement")]
    public static async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
        ILogger logger)
    {
        logger.LogInformation("KoffBot activated. Ready to advertise using AI.");

        var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", EnvironmentVariableTarget.Process);
        if (env != Shared.LocalEnvironmentName)
        {
            await AuthenticationService.Authenticate(req, logger);
        }

        // Run without awaiting to avoid Slack errors to users.
        Task<ObjectResult> task = Task.Run(() => GetAiMessage(logger));
        return new OkResult();
    }

    private static async Task<ObjectResult> GetAiMessage(ILogger logger)
    {
        // Get message from OpenAI.
        using var httpClient = new HttpClient();
        string responseMessage;
        try
        {
            var aiDto = new AiRequestDTO
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
            var parsed = await response.Content.ReadAsAsync<AiResponseDTO>();
            responseMessage = parsed.Choices.First().Text;
        }
        catch (Exception e)
        {
            logger.LogError("Getting data from OpenAI failed.", e);
            var result = new ObjectResult("Getting data from OpenAI failed.")
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };
            return result;
        }

        // Send message to Slack channel.
        var slackDto = new PriceSlackMessageDTO
        {
            Text = responseMessage
        };

        var slackRequest = new HttpRequestMessage(HttpMethod.Post, Shared.GetResponseEndpoint())
        {
            Content = new StringContent(JsonSerializer.Serialize(slackDto), Encoding.UTF8, "application/json")
        };
        await httpClient.SendAsync(slackRequest);

        return new OkObjectResult(null);
    }
}