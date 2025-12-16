using KoffBot.Models;
using KoffBot.Models.Logs;
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

public class KoffBotDrunkFunction
{
    private readonly BlobStorageService _storageService;
    private readonly ILogger _logger;

    public KoffBotDrunkFunction(BlobStorageService storageService, ILoggerFactory loggerFactory)
    {
        _storageService = storageService;
        _logger = loggerFactory.CreateLogger<KoffBotDrunkFunction>();
    }

    [Function("KoffBotDrunk")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequestData req)
    {
        _logger.LogInformation("KoffBot activated. Ready to get rip-roaring drunk.");

        var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", EnvironmentVariableTarget.Process);
        if (env != ResponseEndpointService.LocalEnvironmentName)
        {
            await AuthenticationService.Authenticate(req);
        }

        // Send message to Slack channel.
        using var httpClient = new HttpClient();
        var dto = new DrunkSlackMessage
        {
            Text = "KoffBot drank some delicious Koff beer and is now in 'Drunk Mode' for the next hour. Toasting will be difficult."
        };

        var content = new HttpRequestMessage(HttpMethod.Post, ResponseEndpointService.GetResponseEndpoint())
        {
            Content = new StringContent(JsonSerializer.Serialize(dto), Encoding.UTF8, new MediaTypeHeaderValue("application/json"))
        };
        await httpClient.SendAsync(content);

        // Log the drunkedness.
        try
        {
            var newRow = new DefaultLog
            {
                Created = DateTime.Now,
                CreatedBy = "KoffBotDrunk",
                Modified = DateTime.Now,
                ModifiedBy = "KoffBotDrunk"
            };
            await _storageService.AddAsync(StorageContainers.LogDrunk, newRow);
        }
        catch (Exception e)
        {
            _logger.LogError("Saving into drunkedness log failed. {e}", e);
            var result = req.CreateResponse(HttpStatusCode.OK);
            result.WriteString("Saving into drunkedness log failed.");

            return result;
        }

        return req.CreateResponse(HttpStatusCode.OK);
    }
}
