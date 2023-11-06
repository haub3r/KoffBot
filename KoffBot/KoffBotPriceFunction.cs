using KoffBot.Database;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Unicode;
using System.Threading.Tasks;

namespace KoffBot;

public class KoffBotPriceFunction
{
    private readonly KoffBotContext _dbContext;
    private readonly ILogger _logger;

    public KoffBotPriceFunction(KoffBotContext dbContext, ILoggerFactory loggerFactory)
    {
        _dbContext = dbContext;
        _logger = loggerFactory.CreateLogger<KoffBotPriceFunction>();
    }

    [Function("KoffBotPrice")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequestData req)
    {
        _logger.LogInformation("KoffBot activated. Ready to fetch perfect prices.");

        var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", EnvironmentVariableTarget.Process);
        if (env != Shared.LocalEnvironmentName)
        {
            await AuthenticationService.Authenticate(req, _logger);
        }

        return await GetKoffPrice(_dbContext, _logger, req);
    }

    private static async Task<HttpResponseData> GetKoffPrice(KoffBotContext _dbContext, ILogger logger, HttpRequestData req)
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
            logger.LogError("Getting data from Alko failed.", e);
            var result = req.CreateResponse(HttpStatusCode.InternalServerError);
            result.WriteString("Getting data from Alko failed.");
            
            return result;
        }

        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        using var package = new ExcelPackage(new FileInfo($"{Path.GetTempPath()}\\alkon-hinnasto-tekstitiedostona.xlsx"));

        // Search for current price.
        var price = SearchCurrentPrice(logger, package);

        // Handle price in DB.
        (string lastPrice, bool firstRunToday) = await HandleDbOperations(_dbContext, logger, price);

        // Determine message.
        var message = DetermineMessage(price, lastPrice, firstRunToday);

        var t = Encoding.UTF8.GetBytes($"Koff-tölkin hinta tänään: {price}€{Environment.NewLine}Edellisen tarkistuksen aikainen hinta: {lastPrice}€{Environment.NewLine}{Environment.NewLine}{message}");
        var f = Encoding.UTF8.GetString(t);
        // Send message to Slack channel.
        var dto = new PriceSlackMessageDto
        {
            Text = f
        };

        var content = new HttpRequestMessage(HttpMethod.Post, Shared.GetResponseEndpoint())
        {
            Content = new StringContent(JsonSerializer.Serialize(dto), Encoding.UTF8, "application/json")
        };
        await httpClient.SendAsync(content);

        return req.CreateResponse(HttpStatusCode.OK);
    }

    private static string SearchCurrentPrice(ILogger logger, ExcelPackage package)
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
            var addressLetter = cell.Address.Substring(0, 1);
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

    private async static Task<(string, bool)> HandleDbOperations(KoffBotContext _dbContext, ILogger log, string price)
    {
        var lastPrice = "";
        var firstRunToday = false;
        try
        {
            var lastUpdate = _dbContext.LogPrices.OrderByDescending(d => d.Created)?.FirstOrDefault();
            if (lastUpdate.Created < DateTime.Today)
            {
                firstRunToday = true;
                var newPrice = new LogPrice
                {
                    Amount = price,
                    Created = DateTime.Now,
                    CreatedBy = "KoffBotPrice",
                    Modified = DateTime.Now,
                    ModifiedBy = "KoffBotPrice"
                };
                _dbContext.LogPrices.Add(newPrice);
                await _dbContext.SaveChangesAsync();
            }
            lastPrice = lastUpdate.Amount;
        }
        catch (Exception e)
        {
            log.LogError("Getting the last price failed.", e);
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