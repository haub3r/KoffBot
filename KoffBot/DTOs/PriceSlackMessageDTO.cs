using System.Text.Json.Serialization;

namespace KoffBot;

public class PriceSlackMessageDTO
{
    [JsonPropertyName("text")]
    public string Text { get; set; }
}
