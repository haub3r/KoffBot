using System.Text.Json.Serialization;

namespace KoffBot;

public class EchoSlackMessageDto
{
    [JsonPropertyName("text")]
    public string Text { get; set; }
}
