using System.Text.Json.Serialization;

namespace KoffBot.Models;

public class EchoSlackMessage
{
    [JsonPropertyName("text")]
    public string Text { get; set; }
}
