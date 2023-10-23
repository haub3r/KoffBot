using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;

namespace KoffBot;

class Program
{
    static async Task Main(string[] args)
    {
        var host = new HostBuilder()
            .ConfigureFunctionsWorkerDefaults(builder =>
            {
                builder.UseDefaultWorkerMiddleware();
            })
            .Build();

        await host.RunAsync();
    }
}