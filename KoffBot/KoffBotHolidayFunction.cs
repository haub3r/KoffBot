using KoffBot.Models;
using KoffBot.Messages;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Net;
using System.Text;
using System.Text.Json;

namespace KoffBot;

public class KoffBotHolidayFunction
{
    private readonly IConfiguration _config;
    private readonly ILogger _logger;

    public KoffBotHolidayFunction(IConfiguration config, ILoggerFactory loggerFactory)
    {
        _config = config;
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

            var slackWebHook = _config["SlackWebHook"];
            if (string.IsNullOrEmpty(slackWebHook))
            {
                slackWebHook = ResponseEndpointService.GetResponseEndpoint();
            }

            var content = new HttpRequestMessage(HttpMethod.Post, slackWebHook)
            {
                Content = new StringContent(JsonSerializer.Serialize(message), Encoding.UTF8, "application/json")
            };
            await httpClient.SendAsync(content);
            break;
        }
    }
}
