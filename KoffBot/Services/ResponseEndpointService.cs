namespace KoffBot;

public static class ResponseEndpointService
{
    public static string LocalEnvironmentName = "Local";

    public static string GetResponseEndpoint()
    {
        return Environment.GetEnvironmentVariable("SlackWebHook");
    }
}
