using System.Text.Json.Serialization;

namespace KoffBot.Models.Messages;

public class ToastSlackMessage
{
    [JsonPropertyName("text")]
    public string Text { get; set; }
}
