using System.Text.Json.Serialization;

namespace KoffBot.Models;

public class AiRequest
{
    [JsonPropertyName("model")]
    public string Model { get; set; }

    [JsonPropertyName("messages")]
    public List<AiMessage> Messages { get; set; }

    [JsonPropertyName("temperature")]
    public double Temperature { get; set; }

    [JsonPropertyName("max_tokens")]
    public int MaxTokens { get; set; }
}

public class AiMessage
{
    [JsonPropertyName("role")]
    public string Role { get; set; }

    [JsonPropertyName("content")]
    public string Content { get; set; }
}

public class AiResponse
{
    [JsonPropertyName("choices")]
    public List<AiResponseChoices> Choices { get; set; }
}

public class AiResponseChoices
{
    [JsonPropertyName("message")]
    public AiMessage Message { get; set; }
}
