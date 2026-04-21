using KoffBot.Models;
using KoffBot.Models.Messages;
using KoffBot.Messages;
using KoffBot.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Text.Json;

namespace KoffBot;

public class KoffBotHolidayFunction
{
    private readonly BlobStorageService _storageService;
    private readonly MessagingService _slackService;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger _logger;

    public KoffBotHolidayFunction(BlobStorageService storageService, MessagingService slackService, IHttpClientFactory httpClientFactory, ILoggerFactory loggerFactory)
    {
        _storageService = storageService;
        _slackService = slackService;
        _httpClientFactory = httpClientFactory;
        _logger = loggerFactory.CreateLogger<KoffBotHolidayFunction>();
    }

    [Function("KoffBotHoliday")]
    public async Task Run([TimerTrigger("%TimerTriggerScheduleHolidayFunction%")] TimerInfo myTimer)
    {
        _logger.LogInformation("KoffBot activated. Ready to check for holidays.");

        var holidays = await GetHolidaysAsync();
        var today = DateTime.Today;
        var tomorrow = today.AddDays(1);

        // Check if tomorrow is a holiday (pre-celebration message).
        foreach (var holiday in holidays)
        {
            var parsedDate = DateTime.Parse(holiday.Date, new CultureInfo("fi-FI"));
            if (tomorrow == parsedDate)
            {
                var preCelebration = HolidayMessages.PreCelebrationPossibilities
                    .Where(m => m.Key == holiday.Name).SingleOrDefault();

                if (preCelebration.Value is not null)
                {
                    var message = new HolidaySlackMessage { Text = preCelebration.Value };
                    await _slackService.PostMessageAsync(message);
                }
                break;
            }
        }

        // Check if today is a holiday.
        foreach (var holiday in holidays)
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

    private async Task<List<HolidayApiResponseDetails>> GetHolidaysAsync()
    {
        var year = DateTime.Now.Year;

        var stored = await _storageService.GetJsonAsync<List<HolidayApiResponseDetails>>(
            StorageContainers.Holidays, $"{year}.json");

        if (stored is not null && stored.Count > 0)
        {
            _logger.LogInformation("Using {Count} stored holidays for year {Year}.", stored.Count, year);
            return stored;
        }

        // Fallback: fetch from API and store.
        _logger.LogInformation("No stored holidays found for year {Year}. Fetching from API.", year);
        return await FetchAndStoreHolidaysAsync(year);
    }

    private async Task<List<HolidayApiResponseDetails>> FetchAndStoreHolidaysAsync(int year)
    {
        using var httpClient = _httpClientFactory.CreateClient();

        var finnishHolidaysEndpoint = $"https://api.boffsaopendata.fi/bankingcalendar/v1/api/v1/BankHolidays?year={year}&pageNumber=1&pageSize=50";
        var response = await httpClient.GetAsync(finnishHolidaysEndpoint);
        var responseJson = await response.Content.ReadAsStringAsync();

        var holidays = JsonSerializer.Deserialize<HolidayApiResponse>(responseJson);
        holidays.SpecialDates.Add(new HolidayApiResponseDetails
        {
            Date = $"14.10.{year}",
            Name = "KoffBotSyntymäpäivä"
        });

        await _storageService.SetJsonAsync(StorageContainers.Holidays, $"{year}.json", holidays.SpecialDates);
        _logger.LogInformation("Stored {Count} holidays for year {Year}.", holidays.SpecialDates.Count, year);

        return holidays.SpecialDates;
    }
}
