using KoffBot.Models;
using KoffBot.Models.Messages;
using KoffBot.Messages;
using KoffBot.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Globalization;

namespace KoffBot;

public class KoffBotHolidayFunction
{
    private readonly HolidayService _holidayService;
    private readonly MessagingService _slackService;
    private readonly ILogger _logger;

    public KoffBotHolidayFunction(HolidayService holidayService, MessagingService slackService, ILoggerFactory loggerFactory)
    {
        _holidayService = holidayService;
        _slackService = slackService;
        _logger = loggerFactory.CreateLogger<KoffBotHolidayFunction>();
    }

    [Function("KoffBotHoliday")]
    public async Task Run([TimerTrigger("%TimerTriggerScheduleHolidayFunction%")] TimerInfo myTimer)
    {
        _logger.LogInformation("KoffBot activated. Ready to check for holidays.");

        var holidays = await _holidayService.GetHolidaysAsync();
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
                    var message = new SlackMessage { Text = preCelebration.Value };
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
            var message = new SlackMessage
            {
                Text = slackMessage
            };

            await _slackService.PostMessageAsync(message);
            break;
        }
    }
}
