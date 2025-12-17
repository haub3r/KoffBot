using System.Text.Json.Serialization;

namespace KoffBot.Models.Messages;

public class FridaySlackMessage
{
    [JsonPropertyName("text")]
    public string Text { get; set; }
}
