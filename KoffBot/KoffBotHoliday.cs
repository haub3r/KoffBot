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
            "Palmusunnuntai" => "T�n��n tulee virpojia! Tarjotkaa siis kaikille ovella k�vij�ille ainakin yksi Koff! :koff: (KoffBot ei vastaa alkoholilain pyk�l�n 90� kohdasta 2 aiheutuvista seurauksista jos henkil�/henkil�t keille virvoitusjuomaa [Oluttuote: Sinebrychoff - Koff] tarjotaan ovat alaik�isi� ja/tai alkoholiriippuvaisia)",
            "Pitk�perjantai" => "Hyv�� pitk�perjantaita! Kristus uhrautui jotta saisimme yhden vapaap�iv�n lis��! :buddy-jesus: Koff!",
            "P��si�issunnuntai" => "P��si�inen! Jeesus on yl�snoussut! K�y siis korkkaamassa H�nen kanssaan yksi Koff! :koff: Mutta �l� kerro H�nen faijalleen :shushing_face: :koff:",
            "Vappu" => "Ty�l�isten p�iv� eli vappu! Ota siis sen kunniaksi ainakin yksi Koff! (Sinebrychoff ei tunnista vappua kansalliseksi juhlap�iv�ksi eik� p�iv�n nimellinen merkitys oikeuta ty�ntekij�� pit�m��n vapaap�iv��. Poissaolo p�iv�n aikana lasketaan ty�sopimuksen vastaiseksi ja voi johtaa ty�ntekij�n v�litt�m��n irtisanomiseen ty�teht�vist�. T�m�n viestin lukeminen ei oikeuta ty�ntekij�� pit�m��n taukoa.)",
            "Helatorstai" => "Hyv�� helatorstaita! T�ll� p�iv�ll� ei ole mit��n merkityst�, kuten ei sinullakaan! Aika avata yksi Koff? :koff:",
            "Helluntai" => "Hyv�� helluntaita! P�iv� jolloin vuodatettiin Pyh�� Henke�! Tarvitseeko t�h�n kirjoittaa enemp��? :koff: :koff: :koff:",
            "Pyh�inp�iv�" => "Huuuuuuuuu! :ghost: Hyv�� Halloweenia! Muistakaa korkata (v�hint��n) yksi Koff!",
            "Juhannus" => "Hyv�� juhannusta! :koff: :koff: :koff: :koff: :koff: :koff: :koff: :koff: :koff: :koff: :koff: :koff: :koff: :koff: :koff: :koff: :koff: :koff: :koff: :koff: :koff: :koff: :koff: :koff: :koff: :koff: :koff: :koff: :koff: :koff:",
            "Suomen itsen�isyysp�iv�" => "Hyv�� itsen�isyysp�iv��! Perhe, is�nmaa ja Koff! :koff:",
            "Joulup�iv�" => "Hyv�� ja onnellista joulua toivottaa KoffBot! :santa: :koff:",
            "Uudenvuodenp�iv�" => "Hyv�� uuttavuotta! Eilen taisi menn� muutama Koff! :koff: Uuteen nousuun? Koff!",
            _ => null
        };
    }
}
