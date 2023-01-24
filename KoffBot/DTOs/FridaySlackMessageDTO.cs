using System.Text.Json.Serialization;

namespace KoffBot;

public class FridaySlackMessageDTO
{
    [JsonPropertyName("text")]
    public string Text { get; set; }
}
