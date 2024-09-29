using System.Text.Json.Serialization;

namespace KoffBot.Models;

public class DrunkSlackMessage
{
    [JsonPropertyName("text")]
    public string Text { get; set; }
}
