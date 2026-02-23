using System.Text;
using System.Text.Json;

namespace KoffBot.Services;

public class MessagingService
{
	private readonly IHttpClientFactory _httpClientFactory;

	public MessagingService(IHttpClientFactory httpClientFactory)
	{
		_httpClientFactory = httpClientFactory;
	}

	public async Task PostMessageAsync<T>(T message)
	{
		var endpoint = ResponseEndpointService.GetResponseEndpoint();
		using var client = _httpClientFactory.CreateClient();
		var json = JsonSerializer.Serialize(message);
		var content = new StringContent(json, Encoding.UTF8, "application/json");
		await client.PostAsync(endpoint, content);
	}
}
