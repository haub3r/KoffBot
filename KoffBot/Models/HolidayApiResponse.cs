using System.Text.Json.Serialization;

namespace KoffBot.Models;

public class HolidayApiResponse
{
    [JsonPropertyName("specialDates")]
    public List<HolidayApiResponseDetails> SpecialDates { get; set; }
}

public class HolidayApiResponseDetails
{
    [JsonPropertyName("date")]
    public string Date { get; set; }
    [JsonPropertyName("nameFI")]
    public string Name { get; set; }
}