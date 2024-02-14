using System.Text.Json.Serialization;

namespace KoffBot.Dtos;

public class HolidaySlackMessageDto
{
    [JsonPropertyName("text")]
    public string Text { get; set; }
}
