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
            //.ConfigureServices(s =>
            //{
            //    s.AddSingleton<IHttpResponderService, DefaultHttpResponderService>();
            //})
            .Build();

        await host.RunAsync();
    }
}