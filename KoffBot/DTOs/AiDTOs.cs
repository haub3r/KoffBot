using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace KoffBot;

public class AiRequestDTO
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

public class AiResponseDTO
{
    [JsonPropertyName("choices")]
    public List<AiResponseChoicesDTO> Choices { get; set; }
}

public class AiResponseChoicesDTO
{
    [JsonPropertyName("text")]
    public string Text { get; set; }
}
