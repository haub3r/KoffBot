using System.Text.Json.Serialization;

namespace KoffBot;

public class FridaySlackMessageDto
{
    [JsonPropertyName("text")]
    public string Text { get; set; }
}
