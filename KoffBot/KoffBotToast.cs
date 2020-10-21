using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
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
            var content = new HttpRequestMessage(HttpMethod.Post, "https://hooks.slack.com/services/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")
            {
                Content = new StringContent("{\"text\": \"Koff!\" }", Encoding.UTF8, "application/json")
            };
            await client.SendAsync(content);

            return new OkResult();
        }
    }
}
