using KoffBot.Database;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text;
using System.Text.Json;

namespace KoffBot;

public class KoffBotDrunkFunction
{
    private readonly KoffBotContext _dbContext;
    private readonly ILogger _logger;

    public KoffBotDrunkFunction(KoffBotContext dbContext, ILoggerFactory loggerFactory)
    {
        _dbContext = dbContext;
        _logger = loggerFactory.CreateLogger<KoffBotDrunkFunction>();
    }

    [Function("KoffBotDrunk")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequestData req)
    {
        _logger.LogInformation("KoffBot activated. Ready to get rip-roaring drunk.");
        
        var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", EnvironmentVariableTarget.Process);
        if (env != Shared.LocalEnvironmentName)
        {
            await AuthenticationService.Authenticate(req);
        }

        // Send message to Slack channel.
        using var httpClient = new HttpClient();
        var dto = new DrunkSlackMessageDto
        {
            Text = "KoffBot drank some delicious Koff beer and is now in 'Drunk Mode' for the next hour. Toasting will be difficult."
        };

        var content = new HttpRequestMessage(HttpMethod.Post, Shared.GetResponseEndpoint())
        {
            Content = new StringContent(JsonSerializer.Serialize(dto), Encoding.UTF8, "application/json")
        };
        await httpClient.SendAsync(content);

        // Log the drunkedness.
        try
        {
            var newRow = new LogDrunk
            {
                Created = DateTime.Now,
                CreatedBy = "KoffBotDrunk",
                Modified = DateTime.Now,
                ModifiedBy = "KoffBotDrunk"
            };
            _dbContext.Add(newRow);
            _dbContext.SaveChanges();
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
