using System.Text.Json.Serialization;

namespace KoffBot.Models;

public class ToastSlackMessage
{
    [JsonPropertyName("text")]
    public string Text { get; set; }
}
