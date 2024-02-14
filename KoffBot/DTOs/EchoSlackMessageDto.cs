using System.Text.Json.Serialization;

namespace KoffBot.Dtos;

public class EchoSlackMessageDto
{
    [JsonPropertyName("text")]
    public string Text { get; set; }
}
