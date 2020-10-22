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
    public static class KoffBotUntappd
    {
        [FunctionName("KoffBotUntappd")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("KoffBot activated. Ready to advertise untappd.");

            var client = new HttpClient();
            var content = new HttpRequestMessage(HttpMethod.Post, Shared.GetResponseEndpoint())
            {
                Content = new StringContent("{\"text\": \"Muista, että voit arvostella koffin untappd sovelluksessa. Sovelluksen saa osoitteesta: www.untappd.com. Annathan koffille viisi tähteä :koff:\" }", Encoding.UTF8, "application/json")
            };
            await client.SendAsync(content);

            return new OkResult();
        }
    }
}
