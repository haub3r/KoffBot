using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Text;

namespace KoffBot
{
    public static class KoffBotFriday
    {
        [FunctionName("KoffBotFriday")]
        public static async Task Run([TimerTrigger("0 1 0 * * 5")] TimerInfo myTimer, ILogger log)
        {
            log.LogInformation("KoffBot activated. Ready to hail friday.");

            var client = new HttpClient();
            var content = new HttpRequestMessage(HttpMethod.Post, System.Environment.GetEnvironmentVariable("SlackWebHook"))
            {
                Content = new StringContent("{\"text\": \"Se on perjantai! :meow_party: Mikset juhlisi ottamalla Koff! :koff:\" }", Encoding.UTF8, "application/json")
            };
            await client.SendAsync(content);
        }
    }
}
