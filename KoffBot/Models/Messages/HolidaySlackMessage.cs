using System.Text.Json.Serialization;

namespace KoffBot.Models.Messages;

public class HolidaySlackMessage
{
    [JsonPropertyName("text")]
    public string Text { get; set; }
}
