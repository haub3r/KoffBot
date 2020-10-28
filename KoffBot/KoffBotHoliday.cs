using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Text;
using System;
using System.Text.Json;
using System.Collections.Generic;

namespace KoffBot
{
    public static class KoffBotHoliday
    {
        [FunctionName("KoffBotHoliday")]
#if DEBUG
        public static async Task Run([TimerTrigger("0 */5 * * * *")] TimerInfo myTimer, ILogger log)
#else
        public static async Task Run([TimerTrigger("0 0 0 * * *")] TimerInfo myTimer, ILogger log)
#endif
        {
            log.LogInformation("KoffBot activated. Ready to check for holidays.");

            var client = new HttpClient();
            var finnishHolidaysEndpoint = "http://www.webcal.fi/cal.php?id=1&format=json&start_year=current_year&end_year=current_year&tz=Europe%2FHelsinki";
            var response = await client.GetAsync(finnishHolidaysEndpoint);
            var json = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            };

            var holidays = JsonSerializer.Deserialize<List<HolidayDTO>>(json, options);
            foreach (var holiday in holidays)
            {
                if (DateTime.Today == holiday.Date)
                {
                    var message = ResolveHolidayMessage(holiday);
                    var content = new HttpRequestMessage(HttpMethod.Post, Shared.GetResponseEndpoint())
                    {
                        Content = new StringContent("{\"text\": \"" + message + "\" }", Encoding.UTF8, "application/json")
                    };
                    await client.SendAsync(content);

                    break;
                }
            }
        }

        private static string ResolveHolidayMessage(HolidayDTO holiday)
        {
            switch (holiday.Name)
            {
                case "Pyh‰inp‰iv‰":
                    return "Huuuuuuuuu! :ghost: Hyv‰‰ Halloweenia! Muistakaa korkata (v‰hint‰‰n) yksi Koff!";
                case "Suomen itsen‰isyysp‰iv‰":
                    return "Hyv‰‰ itsen‰isyysp‰iv‰‰! Perhe, is‰nmaa ja Koff! :koff:";
                default: 
                    return null;
            }
        }
    }
}
