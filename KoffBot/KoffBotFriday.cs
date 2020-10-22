using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Text;
using System;

namespace KoffBot
{
    public static class KoffBotFriday
    {
        [FunctionName("KoffBotFriday")]
#if DEBUG
        public static async Task Run([TimerTrigger("0 */5 * * * *")] TimerInfo myTimer, ILogger log)
#else
        public static async Task Run([TimerTrigger("0 1 0 * * 5")] TimerInfo myTimer, ILogger log)
#endif
        {
            log.LogInformation("KoffBot activated. Ready to hail friday.");

            var client = new HttpClient();
            Random random = new Random();
            int randomIndex = random.Next(0, Messages.FridayPossibilities.Length);
            var content = new HttpRequestMessage(HttpMethod.Post, Environment.GetEnvironmentVariable("SlackWebHook"))
            {
                Content = new StringContent("{\"text\": \"" + Messages.FridayPossibilities[randomIndex] + "\" }", Encoding.UTF8, "application/json")
            };
            await client.SendAsync(content);
        }
    }
}
