using KoffBot.Models;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace KoffBot.Services;

public class HolidayService
{
    private readonly BlobStorageService _storageService;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger _logger;

    public HolidayService(BlobStorageService storageService, IHttpClientFactory httpClientFactory, ILoggerFactory loggerFactory)
    {
        _storageService = storageService;
        _httpClientFactory = httpClientFactory;
        _logger = loggerFactory.CreateLogger<HolidayService>();
    }

    public async Task<List<HolidayApiResponseDetails>> GetHolidaysAsync()
    {
        var year = DateTime.Now.Year;

        var stored = await _storageService.GetJsonAsync<List<HolidayApiResponseDetails>>(
            StorageContainers.Holidays, $"{year}.json");

        if (stored is not null && stored.Count > 0)
        {
            _logger.LogInformation("Using {Count} stored holidays for year {Year}.", stored.Count, year);
            return stored;
        }

        _logger.LogInformation("No stored holidays found for year {Year}. Fetching from API.", year);
        return await FetchAndStoreHolidaysAsync(year);
    }

    public async Task<List<HolidayApiResponseDetails>> FetchAndStoreHolidaysAsync(int year)
    {
        using var httpClient = _httpClientFactory.CreateClient();

        var finnishHolidaysEndpoint = $"https://api.boffsaopendata.fi/bankingcalendar/v1/api/v1/BankHolidays?year={year}&pageNumber=1&pageSize=50";
        var response = await httpClient.GetAsync(finnishHolidaysEndpoint);
        response.EnsureSuccessStatusCode();
        var responseJson = await response.Content.ReadAsStringAsync();

        var holidays = JsonSerializer.Deserialize<HolidayApiResponse>(responseJson)
            ?? throw new InvalidOperationException("Failed to deserialize holiday API response.");

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
