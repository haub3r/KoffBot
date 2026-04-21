using KoffBot.Models;
using KoffBot.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace KoffBot;

public class KoffBotHolidaySyncFunction
{
    private readonly BlobStorageService _storageService;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger _logger;

    public KoffBotHolidaySyncFunction(BlobStorageService storageService, IHttpClientFactory httpClientFactory, ILoggerFactory loggerFactory)
    {
        _storageService = storageService;
        _httpClientFactory = httpClientFactory;
        _logger = loggerFactory.CreateLogger<KoffBotHolidaySyncFunction>();
    }

    [Function("KoffBotHolidaySync")]
    public async Task Run([TimerTrigger("%TimerTriggerScheduleHolidaySyncFunction%")] TimerInfo myTimer)
    {
        _logger.LogInformation("KoffBot activated. Ready to sync holidays.");

        var year = DateTime.Now.Year;

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
        _logger.LogInformation("Synced {Count} holidays for year {Year}.", holidays.SpecialDates.Count, year);
    }
}
