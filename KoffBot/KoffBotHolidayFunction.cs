﻿using KoffBot.Dtos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Net;
using System.Text;
using System.Text.Json;

namespace KoffBot;

public class KoffBotHolidayFunction
{
    private readonly ILogger _logger;

    public KoffBotHolidayFunction(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<KoffBotHolidayFunction>();
    }

    [Function("KoffBotHoliday")]
    public async Task Run([TimerTrigger("%TimerTriggerScheduleHolidayFunction%")] TimerInfo myTimer)
    {
        _logger.LogInformation("KoffBot activated. Ready to check for holidays.");

        var clientHandler = new HttpClientHandler() { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate };
        using var httpClient = new HttpClient(clientHandler);

        var finnishHolidaysEndpoint = $"https://api.boffsaopendata.fi/bankingcalendar/v1/api/v1/BankHolidays?year={DateTime.Now.Year}&pageNumber=1&pageSize=50";
        var response = await httpClient.GetAsync(finnishHolidaysEndpoint);
        var json = await response.Content.ReadAsStringAsync();

        var holidays = JsonSerializer.Deserialize<HolidayInboundApiDto>(json);
        var today = DateTime.Today;
        foreach (var holiday in holidays.SpecialDates)
        {
            var parsedDate = DateTime.Parse(holiday.Date, new CultureInfo("fi-FI"));
            if (today != parsedDate)
            {
                continue;
            }

            var slackMessage = ResolveHolidayMessage(holiday.Name);
            var message = new HolidaySlackMessageDto
            {
                Text = slackMessage
            };

            var content = new HttpRequestMessage(HttpMethod.Post, Shared.GetResponseEndpoint())
            {
                Content = new StringContent(JsonSerializer.Serialize(message), Encoding.UTF8, "application/json")
            };
            await httpClient.SendAsync(content);
            break;
        }
    }

    private static string ResolveHolidayMessage(string holidayName)
    {
        return holidayName switch
        {
            "Uudenvuodenpäivä" => "Hyvää uuttavuotta! Eilen taisi mennä muutama Koff! :koff: Uuteen nousuun? Koff!",
            "Pitkäperjantai" => "Hyvää pitkäperjantaita! Kristus uhrautui jotta saisimme yhden vapaapäivän lisää! :buddy-jesus: Koff!",
            "Toinen pääsiäispäivä" => "Pääsiäinen! Jeesus on ylösnoussut! Käy siis korkkaamassa Hänen kanssaan yksi Koff! :koff: Mutta älä kerro Hänen faijalleen :shushing_face: :koff:",
            "Vappu" => "Työläisten päivä eli vappu! Ota siis sen kunniaksi ainakin yksi Koff! (Sinebrychoff ei tunnista vappua kansalliseksi juhlapäiväksi eikä päivän nimellinen merkitys oikeuta työntekijää pitämään vapaapäivää. Poissaolo päivän aikana lasketaan työsopimuksen vastaiseksi ja voi johtaa työntekijän välittömään irtisanomiseen työtehtävistä. Tämän viestin lukeminen ei oikeuta työntekijää pitämään taukoa.)",
            "Helatorsta" => "Hyvää helatorstaita! Tällä päivällä ei ole mitään merkitystä, kuten ei sinullakaan! Aika avata yksi Koff? :koff:",
            "Juhannusaatto" => "Hyvää juhannusta! :koff: :koff: :koff: :koff: :koff: :koff: :koff: :koff: :koff: :koff: :koff: :koff: :koff: :koff: :koff: :koff: :koff: :koff: :koff: :koff: :koff: :koff: :koff: :koff: :koff: :koff: :koff: :koff: :koff: :koff:",
            "Itsenäisyyspäivä" => "Hyvää itsenäisyyspäivää! Perhe, isänmaa ja Koff! :flag-fi: :koff: :ak47:",
            "Jouluaatto" => "Hyvää ja onnellista joulua toivottaa KoffBot! :santa: :koff:",
            "Tapaninpäivä" => "Tapaninpäivä on ensimmäisen marttyyrin Stefanoksen ja myös kaikkien muiden marttyyrien muistopäivä. Suomessa tapaninpäivästä on vuosien saatossa tullut myös hevosten ja hevosmiesten juhlapäivä. Mikä siis parempi päivä laittaa joululahjarahat oikealle hevoselle! Mene osoitteeseen unibet.com ja syötä promokoodi 'KOFFBOTSPECIAL' tienaaksesi 5$ ilmaisina vedonlyöntikrediitteinä! Koff! :koff:",
            _ => null
        };
    }
}
