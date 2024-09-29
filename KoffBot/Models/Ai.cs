using System.Text.Json.Serialization;

namespace KoffBot.Models;

public class AiRequest
{
    [JsonPropertyName("model")]
    public string Model { get; set; }

    [JsonPropertyName("prompt")]
    public string Prompt { get; set; }

    [JsonPropertyName("temperature")]
    public double Temperature { get; set; }

    [JsonPropertyName("max_tokens")]
    public int MaxTokens { get; set; }
}

public class AiResponse
{
    [JsonPropertyName("choices")]
    public List<AiResponseChoices> Choices { get; set; }
}

public class AiResponseChoices
{
    [JsonPropertyName("text")]
    public string Text { get; set; }
}
