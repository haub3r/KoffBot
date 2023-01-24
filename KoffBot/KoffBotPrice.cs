using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Text;
using System;
using System.Data.SqlClient;
using System.Net;
using OfficeOpenXml;
using System.IO;
using System.Linq;
using System.Globalization;

namespace KoffBot;

public static class KoffBotPrice
{
    [FunctionName("KoffBotPrice")]
    public static async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
        ILogger log)
    {
        log.LogInformation("KoffBot activated. Ready to fetch perfect prices.");
#if !DEBUG
        await AuthenticationService.Authenticate(req, log);
#endif
        // Get data from Alko.
        try
        {
            using (var webClient = new WebClient())
            {
                var url = "https://www.alko.fi/INTERSHOP/static/WFS/Alko-OnlineShop-Site/-/Alko-OnlineShop/fi_FI/Alkon%20Hinnasto%20Tekstitiedostona/alkon-hinnasto-tekstitiedostona.xlsx";
                webClient.DownloadFile(url, Path.GetTempPath() + "\\" + "alkon-hinnasto-tekstitiedostona.xlsx");
            }
        }
        catch (Exception e)
        {
            log.LogError("Getting data from Alko failed.", e);
            var result = new ObjectResult("Getting data from Alko failed.")
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };
            return result;
        }

        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        using var package = new ExcelPackage(new FileInfo(Path.GetTempPath() + "\\" + "alkon-hinnasto-tekstitiedostona.xlsx"));

        // Search for current price.
        var firstSheet = package.Workbook.Worksheets.First();

        var koffCell =
        from cells in firstSheet.Cells
        where cells.Value.ToString() == "Koff tölkki"
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

        // Handle price in DB.
        var lastPrice = "";
        var firstTriggeringToday = false;
        try
        {
            var connectionString = Environment.GetEnvironmentVariable("DbConnectionString");
            using SqlConnection conn = new SqlConnection(connectionString);

            conn.Open();
            var sqlGet = $@"SELECT TOP 1 * FROM LogPrice ORDER BY id DESC";

            using SqlCommand cmd = new SqlCommand(sqlGet, conn);
            var rows = await cmd.ExecuteReaderAsync();
            var lastDate = new DateTime();
            while (await rows.ReadAsync())
            {
                lastPrice = rows[1].ToString();
                lastDate = Convert.ToDateTime(rows[3]);
            }

            rows.Close();

            if (lastDate.Date < DateTime.Today)
            {
                firstTriggeringToday = true;
                var sqlSave = $@"INSERT INTO LogPrice (Amount, Created, CreatedBy, Modified, ModifiedBy)
                             VALUES ({price}, CURRENT_TIMESTAMP, 'KoffBotPrice', CURRENT_TIMESTAMP, 'KoffBotPrice')";

                using (SqlCommand cmdSave = new SqlCommand(sqlSave, conn))
                {
                    await cmdSave.ExecuteNonQueryAsync();
                }
            }
        }
        catch (Exception e)
        {
            log.LogError("Getting the last price failed.", e);
            var result = new ObjectResult("Getting the last price failed.")
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };
            return result;
        }

        // Determine message.
        var priceAsDec = Convert.ToDecimal(price, CultureInfo.InvariantCulture);
        var lastPriceAsDec = Convert.ToDecimal(lastPrice, CultureInfo.InvariantCulture);
        var historyMessage = "";
        if (!firstTriggeringToday)
        {
            historyMessage = "Tarkistit jo hinnan aikaisemmin tänään! Sinulla on selvästi jano, miten olisi yksi Koff? :koff:";
        }
        else if (priceAsDec < lastPriceAsDec)
        {
            historyMessage = "Nyt on siis entistä helpompaa ottaa yksi Koff! :koff:";
        }
        else if (priceAsDec > lastPriceAsDec)
        {
            historyMessage = "Mutta se ei haittaa, hinta on laadun merkki! :koff:";
        }
        else
        {
            historyMessage = "Samaan hintaan kuin aina! :koff:";
        }

        // Send message to Slack channel.
        var message = @"{""text"":""Koff-tölkin hinta tänään: " + price + "€\n" + "Eilisen hinta: " + lastPrice + "€\n\n" + historyMessage + @"""}";
        using (var httpClient = new HttpClient())
        {
            var content = new HttpRequestMessage(HttpMethod.Post, Shared.GetResponseEndpoint())
            {
                Content = new StringContent(message, Encoding.UTF8, "application/json")
            };
            await httpClient.SendAsync(content);
        }

        return new OkResult();
    }
}
