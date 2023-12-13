using System.Text.Json.Serialization;

namespace KoffBot;

public class HolidaySlackMessageDto
{
    [JsonPropertyName("text")]
    public string Text { get; set; }
}
