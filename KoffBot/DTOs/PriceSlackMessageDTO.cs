using System.Text.Json.Serialization;

namespace KoffBot;

public class PriceSlackMessageDto
{
    [JsonPropertyName("text")]
    public string Text { get; set; }
}
