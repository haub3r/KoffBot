using System.Text.Json.Serialization;

namespace KoffBot.Models.Messages;

public class PriceSlackMessage
{
    [JsonPropertyName("text")]
    public string Text { get; set; }
}
