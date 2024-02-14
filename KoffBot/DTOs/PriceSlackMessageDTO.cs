using System.Text.Json.Serialization;

namespace KoffBot.Dtos;

public class PriceSlackMessageDto
{
    [JsonPropertyName("text")]
    public string Text { get; set; }
}
