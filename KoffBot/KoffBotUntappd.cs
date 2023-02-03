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

namespace KoffBot;

public static class KoffBotUntappd
{
    [FunctionName("KoffBotUntappd")]
    public static async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
        ILogger logger)
    {
        logger.LogInformation("KoffBot activated. Ready to advertise untappd.");

        var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", EnvironmentVariableTarget.Process);
        if (env != Shared.LocalEnvironmentName)
        {
            await AuthenticationService.Authenticate(req, logger);
        }

        var client = new HttpClient();
        var dto = new UntappdSlackMessageDTO
        {
            Text = $"Muista, että voit arvostella Koffin Untappd-sovelluksessa. Sovelluksen saa osoitteesta: www.untappd.com. Annathan Koffille viisi tähteä :koff:"
        };

        var content = new HttpRequestMessage(HttpMethod.Post, Shared.GetResponseEndpoint())
        {
            Content = new StringContent(JsonSerializer.Serialize(dto), Encoding.UTF8, "application/json")
        };
        await client.SendAsync(content);

        return new OkResult();
    }
}
