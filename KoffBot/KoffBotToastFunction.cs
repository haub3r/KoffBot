using KoffBot.Database;
using KoffBot.Dtos;
using KoffBot.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text;
using System.Text.Json;

namespace KoffBot;

public class KoffBotToastFunction
{
    private readonly KoffBotContext _dbContext;
    private readonly ILogger _logger;

    public KoffBotToastFunction(KoffBotContext dbContext, ILoggerFactory loggerFactory)
    {
        _dbContext = dbContext;
        _logger = loggerFactory.CreateLogger<KoffBotToastFunction>();
    }

    [Function("KoffBotToast")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequestData req)
    {
        _logger.LogInformation("KoffBot activated. Ready for furious toasting.");
        var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", EnvironmentVariableTarget.Process);
        if (env != Shared.LocalEnvironmentName)
        {
            await AuthenticationService.Authenticate(req);
        }

        // Check if in "Drunk Mode".
        var drunkMode = false;
        try
        {
            var lastDrunk = _dbContext.LogDrunks.OrderByDescending(d => d.Created)?.FirstOrDefault().Created;
            if (lastDrunk != null && DateTime.UtcNow < lastDrunk?.AddHours(1))
            {
                drunkMode = true;
            }
        }
        catch (Exception e)
        {
            _logger.LogError("Reading from drunkedness log failed. {e}", e);
            var result = req.CreateResponse(HttpStatusCode.InternalServerError);
            result.WriteString("Reading from drunkedness log failed.");

            return result;
        }

        // Send message to Slack channel.
        var message = new ToastSlackMessageDto()
        {
            Text = "Koff!"
        };

        if (drunkMode)
        {
            message.Text = ScrambleWord(message.Text);
        }

        using var httpClient = new HttpClient();
        var content = new HttpRequestMessage(HttpMethod.Post, Shared.GetResponseEndpoint())
        {
            Content = new StringContent(JsonSerializer.Serialize(message), Encoding.UTF8, "application/json")
        };
        await httpClient.SendAsync(content);

        // Log the toasting.
        try
        {
            var newRow = new LogToast
            {
                Created = DateTime.Now,
                CreatedBy = "KoffBotToast",
                Modified = DateTime.Now,
                ModifiedBy = "KoffBotToast"
            };
            await _dbContext.AddAsync(newRow);
            await _dbContext.SaveChangesAsync();
        }
        catch (Exception e)
        {
            _logger.LogError("Saving into toasting log failed. {e}", e);
            var result = req.CreateResponse(HttpStatusCode.InternalServerError);
            result.WriteString("Saving into toasting log failed.");

            return result;
        }

        return req.CreateResponse(HttpStatusCode.OK);
    }

    private static string ScrambleWord(string str)
    {
        var rand = new Random(); 
        var list = new SortedList<int, char>();
        foreach (var c in str)
            list.Add(rand.Next(), c);
        
        return new string(list.Values.ToArray());
    }
}