namespace KoffBot;

public static class Shared
{
    public static string LocalEnvironmentName = "Local";
    public static string SlackWebhookName = "SlackWebHook";

    public static string GetResponseEndpoint()
    {
        var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", EnvironmentVariableTarget.Process);
        if (env != LocalEnvironmentName)
        {
            return Environment.GetEnvironmentVariable(SlackWebhookName);
        }
        else
        {
            return "http://localhost:7071/koffbottest";
        }
    }
}
