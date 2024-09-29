using System.Text.Json.Serialization;

namespace KoffBot.Models;

public class UntappdSlackMessage
{
    [JsonPropertyName("text")]
    public string Text { get; set; }
}
