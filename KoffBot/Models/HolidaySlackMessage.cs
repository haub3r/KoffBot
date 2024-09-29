using System.Text.Json.Serialization;

namespace KoffBot.Models;

public class HolidaySlackMessage
{
    [JsonPropertyName("text")]
    public string Text { get; set; }
}
