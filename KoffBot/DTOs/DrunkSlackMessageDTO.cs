using System.Text.Json.Serialization;

namespace KoffBot.Dtos;

public class DrunkSlackMessageDto
{
    [JsonPropertyName("text")]
    public string Text { get; set; }
}
