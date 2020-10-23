using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;
using Microsoft.Azure.WebJobs.Extensions.Http;
using System.Threading.Tasks;

namespace KoffBot
{
    public static class AuthenticationService
    {
        public static async Task Authenticate(HttpRequest req, ILogger log)
        {
            req.Headers.TryGetValue("X-Slack-Signature", out var slackSignature);
            req.Headers.TryGetValue("X-Slack-Request-Timestamp", out var slackTimestamp);

            var signingSecret = Environment.GetEnvironmentVariable("SlackSigningSecret");
            var key = Encoding.UTF8.GetBytes(signingSecret);

            string baseString = $"v0:{slackTimestamp}:{await req.ReadAsStringAsync()}";
            using (HMACSHA256 hmac = new HMACSHA256(key))
            {
                var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(baseString));
                var finalBase = "v0=" + ToHexString(computedHash);

                if (finalBase != slackSignature)
                {
                    throw new UnauthorizedAccessException("Access denied.");
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
