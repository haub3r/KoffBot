using KoffBot.Models;
using KoffBot.Models.Messages;
using KoffBot.Messages;
using KoffBot.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Net;
using System.Text.Json;

namespace KoffBot;

public class KoffBotHolidayFunction
{
    private readonly MessagingService _slackService;
    private readonly ILogger _logger;

    public KoffBotHolidayFunction(MessagingService slackService, ILoggerFactory loggerFactory)
    {
        _slackService = slackService;
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
        var responseJson = await response.Content.ReadAsStringAsync();

        var holidays = JsonSerializer.Deserialize<HolidayApiResponse>(responseJson);
        holidays.SpecialDates.Add(new HolidayApiResponseDetails
        {
            Date = $"14.10.{DateTime.Now.Year}",
            Name = "KoffBotSyntymäpäivä"
        });

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
            var message = new HolidaySlackMessage
            {
                Text = slackMessage
            };

            await _slackService.PostMessageAsync(message);
            break;
        }
    }
}
