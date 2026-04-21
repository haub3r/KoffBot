using System.Text.Json.Serialization;

namespace KoffBot.Models.Messages;

public class SlackMessage
{
    [JsonPropertyName("text")]
    public string Text { get; set; }
}
