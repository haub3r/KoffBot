using System.Text.Json.Serialization;

namespace KoffBot;

public class HolidayInboundApiDto
{
    [JsonPropertyName("specialDates")]
    public List<HolidayInboundApiDetailsDto> SpecialDates { get; set; }
}

public class HolidayInboundApiDetailsDto
{
    [JsonPropertyName("date")]
    public string Date { get; set; }
    [JsonPropertyName("nameFI")]
    public string Name { get; set; }
}