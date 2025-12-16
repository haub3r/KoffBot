using KoffBot.Models;
using KoffBot.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace KoffBot;

public class KoffBotStatsFunction
{
    private readonly BlobStorageService _storageService;
    private readonly ILogger _logger;

    public KoffBotStatsFunction(BlobStorageService storageService, ILoggerFactory loggerFactory)
    {
        _storageService = storageService;
        _logger = loggerFactory.CreateLogger<KoffBotStatsFunction>();
    }

    [Function("KoffBotStats")]
    public async Task<Stats> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequestData req)
    {
        _logger.LogInformation("KoffBot activated. Ready to retrieve epic statistics.");

        // Get the stats.
        try
        {
            var drunkCount = await _storageService.GetCountAsync(StorageContainers.LogDrunk);
            var fridayCount = await _storageService.GetCountAsync(StorageContainers.LogFriday);
            var toastCount = await _storageService.GetCountAsync(StorageContainers.LogToast);

            return new Stats
            {
                DrunkCount = drunkCount,
                FridayCount = fridayCount,
                ToastCount = toastCount
            };
        }
        catch (Exception e)
        {
            _logger.LogError("Getting the stats failed. {e}", e);
            throw;
        }
    }
}
