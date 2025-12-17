using System.Text.Json.Serialization;

namespace KoffBot.Models.Messages;

public class EchoSlackMessage
{
    [JsonPropertyName("text")]
    public string Text { get; set; }
}
