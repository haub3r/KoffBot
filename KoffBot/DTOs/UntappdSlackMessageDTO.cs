using System.Text.Json.Serialization;

namespace KoffBot.Dtos;

public class UntappdSlackMessageDto
{
    [JsonPropertyName("text")]
    public string Text { get; set; }
}
