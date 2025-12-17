using System.Text.Json.Serialization;

namespace KoffBot.Models.Messages;

public class DrunkSlackMessage
{
    [JsonPropertyName("text")]
    public string Text { get; set; }
}
