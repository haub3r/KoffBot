using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace KoffBot.Services;

public class MessagingService
{
	private readonly IHttpClientFactory _httpClientFactory;
	private readonly ILogger _logger;

	public MessagingService(IHttpClientFactory httpClientFactory, ILoggerFactory loggerFactory)
	{
		_httpClientFactory = httpClientFactory;
		_logger = loggerFactory.CreateLogger<MessagingService>();
	}

	public async Task PostMessageAsync<T>(T message)
	{
		var endpoint = ResponseEndpointService.GetResponseEndpoint();
		using var client = _httpClientFactory.CreateClient();
		var json = JsonSerializer.Serialize(message);
		var content = new StringContent(json, Encoding.UTF8, "application/json");

		try
		{
			var response = await client.PostAsync(endpoint, content);
			if (!response.IsSuccessStatusCode)
			{
				_logger.LogError("Slack webhook returned {StatusCode}.", response.StatusCode);
			}
		}
		catch (Exception e)
		{
			_logger.LogError(e, "Failed to post message to Slack.");
		}
	}
}
