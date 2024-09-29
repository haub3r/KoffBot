using KoffBot.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureFunctionsWorkerDefaults(builder =>
    {
        builder.UseDefaultWorkerMiddleware();
    })
    .ConfigureServices((hostContext, services) =>
    {
        var connectionString = Environment.GetEnvironmentVariable("DbConnectionString");
        services.AddDbContext<KoffBotContext>(options =>
            options.UseSqlServer(connectionString));
    })
    .Build();

await host.RunAsync();