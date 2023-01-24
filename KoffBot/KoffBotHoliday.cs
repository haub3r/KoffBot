using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Text;
using System;
using System.Text.Json;
using System.Collections.Generic;

namespace KoffBot;

public static class KoffBotHoliday
{
    [Disable]
    [FunctionName("KoffBotHoliday")]
#if DEBUG
    public static async Task Run([TimerTrigger("0 */5 * * * *")] TimerInfo myTimer, ILogger log)
#else
    public static async Task Run([TimerTrigger("0 0 0 * * *")] TimerInfo myTimer, ILogger log)
#endif
    {
        log.LogInformation("KoffBot activated. Ready to check for holidays.");

        var client = new HttpClient();
        var finnishHolidaysEndpoint = "http://www.webcal.fi/cal.php?id=1&format=json&start_year=current_year&end_year=current_year&tz=Europe%2FHelsinki";
        var response = await client.GetAsync(finnishHolidaysEndpoint);
        var json = await response.Content.ReadAsStringAsync();

        var holidays = JsonSerializer.Deserialize<List<HolidayDTO>>(json, Shared.JsonDeserializerOptions);
        foreach (var holiday in holidays)
        {
            if (DateTime.Today == holiday.Date)
            {
                var message = ResolveHolidayMessage(holiday);
                var content = new HttpRequestMessage(HttpMethod.Post, Shared.GetResponseEndpoint())
                {
                    Content = new StringContent("{\"text\": \"" + message + "\" }", Encoding.UTF8, "application/json")
                };
                await client.SendAsync(content);

                break;
            }
        }
    }

    private static string ResolveHolidayMessage(HolidayDTO holiday)
    {
        return holiday.Name switch
        {
            "Palmusunnuntai" => "Tänään tulee virpojia! Tarjotkaa siis kaikille ovella kävijöille ainakin yksi Koff! :koff: (KoffBot ei vastaa alkoholilain pykälän 90§ kohdasta 2 aiheutuvista seurauksista jos henkilö/henkilöt keille virvoitusjuomaa [Oluttuote: Sinebrychoff - Koff] tarjotaan ovat alaikäisiä ja/tai alkoholiriippuvaisia)",
            "Pitkäperjantai" => "Hyvää pitkäperjantaita! Kristus uhrautui jotta saisimme yhden vapaapäivän lisää! :buddy-jesus: Koff!",
            "Pääsiäissunnuntai" => "Pääsiäinen! Jeesus on ylösnoussut! Käy siis korkkaamassa Hänen kanssaan yksi Koff! :koff: Mutta älä kerro Hänen faijalleen :shushing_face: :koff:",
            "Vappu" => "Työläisten päivä eli vappu! Ota siis sen kunniaksi ainakin yksi Koff! (Sinebrychoff ei tunnista vappua kansalliseksi juhlapäiväksi eikä päivän nimellinen merkitys oikeuta työntekijää pitämään vapaapäivää. Poissaolo päivän aikana lasketaan työsopimuksen vastaiseksi ja voi johtaa työntekijän välittömään irtisanomiseen työtehtävistä. Tämän viestin lukeminen ei oikeuta työntekijää pitämään taukoa.)",
            "Helatorstai" => "Hyvää helatorstaita! Tällä päivällä ei ole mitään merkitystä, kuten ei sinullakaan! Aika avata yksi Koff? :koff:",
            "Helluntai" => "Hyvää helluntaita! Päivä jolloin vuodatettiin Pyhää Henkeä! Tarvitseeko tähän kirjoittaa enempää? :koff: :koff: :koff:",
            "Pyhäinpäivä" => "Huuuuuuuuu! :ghost: Hyvää Halloweenia! Muistakaa korkata (vähintään) yksi Koff!",
            "Juhannus" => "Hyvää juhannusta! :koff: :koff: :koff: :koff: :koff: :koff: :koff: :koff: :koff: :koff: :koff: :koff: :koff: :koff: :koff: :koff: :koff: :koff: :koff: :koff: :koff: :koff: :koff: :koff: :koff: :koff: :koff: :koff: :koff: :koff:",
            "Suomen itsenäisyyspäivä" => "Hyvää itsenäisyyspäivää! Perhe, isänmaa ja Koff! :koff:",
            "Joulupäivä" => "Hyvää ja onnellista joulua toivottaa KoffBot! :santa: :koff:",
            "Uudenvuodenpäivä" => "Hyvää uuttavuotta! Eilen taisi mennä muutama Koff! :koff: Uuteen nousuun? Koff!",
            _ => null
        };
    }
}
