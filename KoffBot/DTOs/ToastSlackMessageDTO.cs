using System.Text.Json.Serialization;

namespace KoffBot;

public class ToastSlackMessageDTO
{
    [JsonPropertyName("text")]
    public string Text { get; set; }
}
