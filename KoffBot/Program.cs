using Azure.Storage.Blobs;
using KoffBot.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace KoffBot;

public class Program
{
    private static async Task Main(string[] args)
    {
        await CreateHostBuilder(args).Build().RunAsync();
    }

    public static IHostBuilder CreateHostBuilder(string[] args)
    {
        return Host.CreateDefaultBuilder(args)
            .ConfigureFunctionsWorkerDefaults(builder =>
            {
                builder.UseDefaultWorkerMiddleware();
            })
            .ConfigureServices((hostContext, services) =>
            {
                var connectionString = Environment.GetEnvironmentVariable("BlobStorageConnectionString");
                services.AddSingleton(_ => new BlobServiceClient(connectionString));
                services.AddSingleton<BlobStorageService, BlobStorageService>();
                services.AddHttpClient();
                services.AddSingleton<MessagingService>();
            });
    }
}