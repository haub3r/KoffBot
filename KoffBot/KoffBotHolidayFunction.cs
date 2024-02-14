using KoffBot.Dtos;
using KoffBot.Messages;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Net;
using System.Text;
using System.Text.Json;

namespace KoffBot;

public class KoffBotHolidayFunction
{
    private readonly ILogger _logger;

    public KoffBotHolidayFunction(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<KoffBotHolidayFunction>();
    }

    [Function("KoffBotHoliday")]
    public async Task Run([TimerTrigger("%TimerTriggerScheduleHolidayFunction%")] TimerInfo myTimer)
    {
        _logger.LogInformation("KoffBot activated. Ready to check for holidays.");

        var clientHandler = new HttpClientHandler() { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate };
        using var httpClient = new HttpClient(clientHandler);

        var finnishHolidaysEndpoint = $"https://api.boffsaopendata.fi/bankingcalendar/v1/api/v1/BankHolidays?year={DateTime.Now.Year}&pageNumber=1&pageSize=50";
        var response = await httpClient.GetAsync(finnishHolidaysEndpoint);
        var json = await response.Content.ReadAsStringAsync();

        var holidays = JsonSerializer.Deserialize<HolidayInboundApiDto>(json);
        var today = DateTime.Today;
        foreach (var holiday in holidays.SpecialDates)
        {
            var parsedDate = DateTime.Parse(holiday.Date, new CultureInfo("fi-FI"));
            if (today != parsedDate)
            {
                continue;
            }

            var foundHoliday = HolidayMessages.HolidayPossibilities.Where(m => m.Key == holiday.Name).SingleOrDefault();
            var slackMessage = foundHoliday.Value ?? "";
            var message = new HolidaySlackMessageDto
            {
                Text = slackMessage
            };

            var content = new HttpRequestMessage(HttpMethod.Post, ResponseEndpointService.GetResponseEndpoint())
            {
                Content = new StringContent(JsonSerializer.Serialize(message), Encoding.UTF8, "application/json")
            };
            await httpClient.SendAsync(content);
            break;
        }
    }
}
