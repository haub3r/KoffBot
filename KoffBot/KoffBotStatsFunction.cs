using KoffBot.Database;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace KoffBot;

public class KoffBotStatsFunction
{
    private readonly KoffBotContext _dbContext;
    private readonly ILogger _logger;

    public KoffBotStatsFunction(KoffBotContext dbContext, ILoggerFactory loggerFactory)
    {
        _dbContext = dbContext;
        _logger = loggerFactory.CreateLogger<KoffBotStatsFunction>();
    }

    [Function("KoffBotStats")]
    public async Task<StatsDto> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequestData req)
    {
        _logger.LogInformation("KoffBot activated. Ready to retrieve epic statistics.");

        // Get the stats.
        try
        {
            var result = new StatsDto();
            var drunkCount = await _dbContext.LogDrunks.CountAsync();
            var fridayCount = await _dbContext.LogFridays.CountAsync();
            var toastCount = await _dbContext.LogToasts.CountAsync();

            return new StatsDto
            {
                DrunkCount = drunkCount,
                FridayCount = fridayCount,
                ToastCount = toastCount
            };
        }
        catch (Exception e)
        {
            _logger.LogError("Getting the stats failed.", e);
            throw;
        }
    }
}
