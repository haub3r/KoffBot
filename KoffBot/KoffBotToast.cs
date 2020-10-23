using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Linq;

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
#if !DEBUG
            await AuthenticationService.Authenticate(req, log);
#endif
            var client = new HttpClient();
            var content = new HttpRequestMessage(HttpMethod.Post, Shared.GetResponseEndpoint())
            {
                Content = new StringContent("{\"text\": \"Koff!\" }", Encoding.UTF8, "application/json")
            };
            await client.SendAsync(content);

            return new OkResult();
        }
    }
}
