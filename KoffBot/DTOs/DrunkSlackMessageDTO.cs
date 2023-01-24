using System.Text.Json.Serialization;

namespace KoffBot;

public class DrunkSlackMessageDTO
{
    [JsonPropertyName("text")]
    public string Text { get; set; }
}
