using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Text;

namespace KoffBot
{
    public static class KoffBotToast
    {
        [FunctionName("KoffBotToast")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("KoffBot activated. Ready for furious toasting.");

            var client = new HttpClient();
            var content = new HttpRequestMessage(HttpMethod.Post, System.Environment.GetEnvironmentVariable("SlackWebHook"))
            {
                Content = new StringContent("{\"text\": \"Koff!\" }", Encoding.UTF8, "application/json")
            };
            await client.SendAsync(content);

            return new OkResult();
        }
    }
}
