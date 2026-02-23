using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;

namespace KoffBot.Services;

public class SlackAuthenticationMiddleware : IFunctionsWorkerMiddleware
{
    private static readonly HashSet<string> UnauthenticatedFunctions = ["KoffBotStats"];

    public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
    {
        if (!UnauthenticatedFunctions.Contains(context.FunctionDefinition.Name))
        {
            var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", EnvironmentVariableTarget.Process);
            if (env != ResponseEndpointService.LocalEnvironmentName)
            {
                var httpReqData = await context.GetHttpRequestDataAsync();
                if (httpReqData is not null)
                {
                    await AuthenticationService.Authenticate(httpReqData);
                    httpReqData.Body.Position = 0;
                }
            }
        }

        await next(context);
    }
}
