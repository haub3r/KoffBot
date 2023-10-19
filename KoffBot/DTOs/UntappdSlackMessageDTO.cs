using System.Text.Json.Serialization;

namespace KoffBot;

public class UntappdSlackMessageDto
{
    [JsonPropertyName("text")]
    public string Text { get; set; }
}
