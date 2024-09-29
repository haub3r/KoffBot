using System.Text.Json.Serialization;

namespace KoffBot.Models;

public class PriceSlackMessage
{
    [JsonPropertyName("text")]
    public string Text { get; set; }
}
