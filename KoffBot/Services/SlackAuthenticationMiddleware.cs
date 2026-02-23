using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Security.Authentication;

namespace KoffBot.Services;

public class SlackAuthenticationMiddleware(ILogger<SlackAuthenticationMiddleware> logger) : IFunctionsWorkerMiddleware
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
                    try
                    {
                        await AuthenticationService.Authenticate(httpReqData);
                        httpReqData.Body.Position = 0;
                    }
                    catch (AuthenticationException ex)
                    {
                        logger.LogWarning(ex, "Slack authentication failed for {Function}.", context.FunctionDefinition.Name);
                        var response = httpReqData.CreateResponse(HttpStatusCode.Unauthorized);
                        await response.WriteStringAsync("Authentication failed.");
                        context.GetInvocationResult().Value = response;
                        return;
                    }
                }
            }
        }

        await next(context);
    }
}
