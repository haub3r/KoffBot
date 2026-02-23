using KoffBot.Models;
using KoffBot.Models.Logs;
using KoffBot.Models.Messages;
using KoffBot.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Net;
using System.Text.RegularExpressions;

namespace KoffBot;

public partial class KoffBotPriceFunction
{
    private readonly BlobStorageService _storageService;
    private readonly MessagingService _slackService;
    private readonly ILogger _logger;
    private const string KoffProductUrl = "https://www.alko.fi/fi/tuotteet/718934/koff-a-iv-olut";

    public KoffBotPriceFunction(BlobStorageService storageService, MessagingService slackService, ILoggerFactory loggerFactory)
    {
        _storageService = storageService;
        _slackService = slackService;
        _logger = loggerFactory.CreateLogger<KoffBotPriceFunction>();
    }

    [Function("KoffBotPrice")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequestData req)
    {
        _logger.LogInformation("KoffBot activated. Ready to fetch perfect prices.");

        return await GetKoffPrice(req);
    }

    private async Task<HttpResponseData> GetKoffPrice(HttpRequestData req)
    {
        // Get price from Alko product page.
        string price;
        using var httpClient = new HttpClient();
        try
        {
            httpClient.DefaultRequestHeaders.Add("User-Agent", "KoffBot/1.0");
            var html = await httpClient.GetStringAsync(KoffProductUrl);
            price = ParsePriceFromHtml(html);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Getting data from Alko failed.");
            var result = req.CreateResponse(HttpStatusCode.InternalServerError);
            result.WriteString("Getting data from Alko failed.");

            return result;
        }

        // Handle price in storage.
        (string lastPrice, bool firstRunToday) = await HandleStorageOperations(price);

        // Determine message.
        var message = DetermineMessage(price, lastPrice, firstRunToday);

        var fullMessage = $"Koff-tölkin hinta tänään: {price}€{Environment.NewLine}Edellisen tarkistuksen aikainen hinta: {lastPrice}€{Environment.NewLine}{Environment.NewLine}{message}";

        // Send message to Slack channel.
        var dto = new PriceSlackMessage
        {
            Text = fullMessage,
        };
        await _slackService.PostMessageAsync(dto);

        return req.CreateResponse(HttpStatusCode.OK);
    }

    private static string ParsePriceFromHtml(string html)
    {
        // The product page contains "Hinta X,XX €" in the product info section.
        var match = PriceRegex().Match(html);
        if (!match.Success)
        {
            throw new InvalidOperationException("Could not find Koff price on the Alko product page.");
        }

        // Return the price with dot as decimal separator for consistent storage (e.g. "1.55").
        var priceText = match.Groups[1].Value;
        return priceText.Replace(',', '.');
    }

    [GeneratedRegex(@"Hinta\s+(\d+,\d{2})\s*€")]
    private static partial Regex PriceRegex();

    private async Task<(string, bool)> HandleStorageOperations(string price)
    {
        var lastPrice = "";
        var firstRunToday = false;
        try
        {
            var lastUpdate = await _storageService.GetLatestAsync<PriceLog>(StorageContainers.LogPrice);
            if (lastUpdate == null || lastUpdate.Created < DateTime.Today)
            {
                firstRunToday = true;
                var newPrice = new PriceLog
                {
                    Amount = price,
                    Created = DateTime.UtcNow,
                    CreatedBy = "KoffBotPrice",
                    Modified = DateTime.UtcNow,
                    ModifiedBy = "KoffBotPrice"
                };
                await _storageService.AddAsync(StorageContainers.LogPrice, newPrice);
            }
            lastPrice = lastUpdate?.Amount ?? price;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Getting the last price failed.");
        }

        return (lastPrice, firstRunToday);
    }

    private static string DetermineMessage(string price, string lastPrice, bool firstRunToday)
    {
        var priceAsDec = Convert.ToDecimal(price, CultureInfo.InvariantCulture);
        var lastPriceAsDec = Convert.ToDecimal(lastPrice, CultureInfo.InvariantCulture);
        string message;
        if (!firstRunToday)
        {
            message = "Tarkistit jo hinnan aikaisemmin tänään! Sinulla on selvästi jano, miten olisi yksi Koff? :koff:";
        }
        else if (priceAsDec < lastPriceAsDec)
        {
            message = "Nyt on siis entistä helpompaa ottaa yksi Koff! :koff:";
        }
        else if (priceAsDec > lastPriceAsDec)
        {
            message = "Mutta se ei haittaa, hinta on laadun merkki! :koff:";
        }
        else
        {
            message = "Samaan hintaan kuin aina! :koff:";
        }

        return message;
    }
}