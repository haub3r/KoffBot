using KoffBot.Models;
using KoffBot.Models.Logs;
using KoffBot.Models.Messages;
using KoffBot.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using System.Globalization;
using System.Net;

namespace KoffBot;

public class KoffBotPriceFunction
{
    private readonly BlobStorageService _storageService;
    private readonly MessagingService _slackService;
    private readonly ILogger _logger;

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

        var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", EnvironmentVariableTarget.Process);
        if (env != ResponseEndpointService.LocalEnvironmentName)
        {
            await AuthenticationService.Authenticate(req);
        }

        return await GetKoffPrice(req);
    }

    private async Task<HttpResponseData> GetKoffPrice(HttpRequestData req)
    {
        // Get data from Alko.
        using var httpClient = new HttpClient();
        try
        {
            var url = "https://www.alko.fi/INTERSHOP/static/WFS/Alko-OnlineShop-Site/-/Alko-OnlineShop/fi_FI/Alkon%20Hinnasto%20Tekstitiedostona/alkon-hinnasto-tekstitiedostona.xlsx";
            byte[] fileBytes = await httpClient.GetByteArrayAsync(url);
            File.WriteAllBytes($"{Path.GetTempPath()}\\alkon-hinnasto-tekstitiedostona.xlsx", fileBytes);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Getting data from Alko failed.");
            var result = req.CreateResponse(HttpStatusCode.InternalServerError);
            result.WriteString("Getting data from Alko failed.");

            return result;
        }

        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        using var package = new ExcelPackage(new FileInfo($"{Path.GetTempPath()}\\alkon-hinnasto-tekstitiedostona.xlsx"));

        // Search for current price.
        var price = SearchCurrentPrice(package);

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

    private static string SearchCurrentPrice(ExcelPackage package)
    {
        var firstSheet = package.Workbook.Worksheets.First();

        var koffCell =
            from cells in firstSheet.Cells
            where cells.Value.ToString() == "718934"
            select cells;

        var currentRowNumber = koffCell.First().Start.Row;
        var currentRow =
            from cells in firstSheet.Cells
            where cells.Start.Row == currentRowNumber
            select cells;

        var unitSize = "";
        var price = "";
        var priceByLitre = "";
        foreach (var cell in currentRow)
        {
            var addressLetter = cell.Address[..1];
            switch (addressLetter)
            {
                case "D":
                    unitSize = cell.Text;
                    break;
                case "E":
                    price = cell.Text;
                    break;
                case "F":
                    priceByLitre = cell.Text + "€";
                    break;
                default:
                    break;
            }
        }

        return price;
    }

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