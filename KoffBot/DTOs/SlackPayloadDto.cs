using System.Text.Json.Serialization;

namespace KoffBot;

public class SlackPayloadDto
{
    [JsonPropertyName("channel_name")]
    public string ChannelName { get; set; }

    [JsonPropertyName("user_id")]
    public string UserId { get; set; }

    [JsonPropertyName("user_name")]
    public string UserName { get; set; }

    [JsonPropertyName("text")]
    public string MessageFromUser { get; set; }
}
