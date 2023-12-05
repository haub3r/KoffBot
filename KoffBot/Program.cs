using KoffBot.Database;
using Microsoft.EntityFrameworkCore;
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
                var connectionString = Environment.GetEnvironmentVariable("DbConnectionString");
                services.AddDbContext<KoffBotContext>(options =>
                    options.UseSqlServer(connectionString));
            });
    }
}