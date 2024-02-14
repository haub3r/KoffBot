using System.Text.Json.Serialization;

namespace KoffBot.Dtos;

public class FridaySlackMessageDto
{
    [JsonPropertyName("text")]
    public string Text { get; set; }
}
