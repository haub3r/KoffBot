using KoffBot.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace KoffBot;

public class KoffBotHolidaySyncFunction
{
    private readonly HolidayService _holidayService;
    private readonly ILogger _logger;

    public KoffBotHolidaySyncFunction(HolidayService holidayService, ILoggerFactory loggerFactory)
    {
        _holidayService = holidayService;
        _logger = loggerFactory.CreateLogger<KoffBotHolidaySyncFunction>();
    }

    [Function("KoffBotHolidaySync")]
    public async Task Run([TimerTrigger("%TimerTriggerScheduleHolidaySyncFunction%")] TimerInfo myTimer)
    {
        _logger.LogInformation("KoffBot activated. Ready to sync holidays.");

        var year = DateTime.Now.Year;
        await _holidayService.FetchAndStoreHolidaysAsync(year);
    }
}
