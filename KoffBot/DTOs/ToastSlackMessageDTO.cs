using System.Text.Json.Serialization;

namespace KoffBot.Dtos;

public class ToastSlackMessageDto
{
    [JsonPropertyName("text")]
    public string Text { get; set; }
}
