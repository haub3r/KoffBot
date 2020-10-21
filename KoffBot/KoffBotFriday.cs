using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Text;

namespace KoffBot
{
    public static class KoffBotFriday
    {
        [FunctionName("KoffBotFriday")]
        public static async Task<IActionResult> Run([TimerTrigger("0 1 0 * * 5")] TimerInfo myTimer, ILogger log)
        {
            log.LogInformation("KoffBot activated. Ready to hail friday.");

            var client = new HttpClient();
            var content = new HttpRequestMessage(HttpMethod.Post, "https://hooks.slack.com/services/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")
            {
                Content = new StringContent("{\"text\": \"It's friday! :meow_party: You should celebrate with a Koff! :koff:\" }", Encoding.UTF8, "application/json")
            };
            await client.SendAsync(content);

            return new OkResult();
        }
    }
}
