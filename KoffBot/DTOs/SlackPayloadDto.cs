using System.Text.Json.Serialization;

namespace KoffBot;

public class SlackPayloadDto
{
    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("channel")]
    public string Channel { get; set; }

    [JsonPropertyName("user")]
    public string User { get; set; }

    [JsonPropertyName("text")]
    public string Text { get; set; }
}
