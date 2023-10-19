using System.Text.Json.Serialization;

namespace KoffBot;

public class ToastSlackMessageDto
{
    [JsonPropertyName("text")]
    public string Text { get; set; }
}
