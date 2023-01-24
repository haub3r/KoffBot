using System.Text.Json.Serialization;

namespace KoffBot;

public class UntappdSlackMessageDTO
{
    [JsonPropertyName("text")]
    public string Text { get; set; }
}
