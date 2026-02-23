using Microsoft.Azure.Functions.Worker.Http;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Text;

namespace KoffBot.Services;

public static class AuthenticationService
{
    public static async Task Authenticate(HttpRequestData req)
    {
        req.Headers.TryGetValues("X-Slack-Signature", out var slackSignature);
        req.Headers.TryGetValues("X-Slack-Request-Timestamp", out var slackTimestamp);
        if (string.IsNullOrEmpty(slackSignature.FirstOrDefault()) || string.IsNullOrEmpty(slackTimestamp.FirstOrDefault()))
        {
            throw new AuthenticationException("Access denied. The request was missing one or more Slack headers.");
        }

        var signingSecret = Environment.GetEnvironmentVariable("SlackSigningSecret");
        var key = Encoding.UTF8.GetBytes(signingSecret);

        string baseString = $"v0:{slackTimestamp.First()}:{await req.ReadAsStringAsync()}";
        
        using HMACSHA256 hmac = new(key);
        var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(baseString));
        var finalBase = "v0=" + ToHexString(computedHash);

        if (finalBase != slackSignature.First())
        {
            throw new AuthenticationException("Access denied. The calculated hash did not match request hash.");
        }
    }

    private static string ToHexString(byte[] array)
    {
        StringBuilder hex = new(array.Length * 2);
        foreach (byte b in array)
        {
            hex.AppendFormat("{0:x2}", b);
        }
        return hex.ToString();
    }
}
