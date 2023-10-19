using System.Text.Json.Serialization;

namespace KoffBot;

public class DrunkSlackMessageDto
{
    [JsonPropertyName("text")]
    public string Text { get; set; }
}
