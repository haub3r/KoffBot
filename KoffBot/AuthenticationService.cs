using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;
using Microsoft.Azure.WebJobs.Extensions.Http;
using System.Threading.Tasks;
using System.Security.Authentication;

namespace KoffBot
{
    public static class AuthenticationService
    {
        public static async Task Authenticate(HttpRequest req, ILogger log)
        {
            req.Headers.TryGetValue("X-Slack-Signature", out var slackSignature);
            req.Headers.TryGetValue("X-Slack-Request-Timestamp", out var slackTimestamp);
            if (string.IsNullOrEmpty(slackSignature) || string.IsNullOrEmpty(slackTimestamp))
            {
                throw new AuthenticationException("Access denied. The request was missing one or more Slack headers.");
            }

            var signingSecret = Environment.GetEnvironmentVariable("SlackSigningSecret");
            var key = Encoding.UTF8.GetBytes(signingSecret);

            string baseString = $"v0:{slackTimestamp}:{await req.ReadAsStringAsync()}";
            using (HMACSHA256 hmac = new HMACSHA256(key))
            {
                var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(baseString));
                var finalBase = "v0=" + ToHexString(computedHash);

                if (finalBase != slackSignature)
                {
                    throw new AuthenticationException("Access denied. The calculated hash did not match request hash.");
                }
            }
        }

        public static string ToHexString(byte[] array)
        {
            StringBuilder hex = new StringBuilder(array.Length * 2);
            foreach (byte b in array)
            {
                hex.AppendFormat("{0:x2}", b);
            }
            return hex.ToString();
        }
    }
}
