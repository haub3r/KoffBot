using Azure.Storage.Blobs;
using KoffBot.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace KoffBot;

public class Program
{
	private static async Task Main(string[] args)
	{
		var host = new HostBuilder()
			.ConfigureFunctionsWebApplication(builder =>
			{
				builder.UseMiddleware<SlackAuthenticationMiddleware>();
			})
			.ConfigureServices(services =>
			{
				var connectionString = Environment.GetEnvironmentVariable("BlobStorageConnectionString");
				services.AddSingleton(_ => new BlobServiceClient(connectionString));
				services.AddSingleton<BlobStorageService>();
				services.AddHttpClient();
				services.AddSingleton<MessagingService>();
			})
			.Build();

		await host.RunAsync();
	}
}