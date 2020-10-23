using System;
using System.Collections.Generic;
using System.Text;

namespace KoffBot
{
    public static class Shared
    {
        public static string GetResponseEndpoint()
        {
#if DEBUG
            return "http://localhost:7071/koffbottest";
#else
            return Environment.GetEnvironmentVariable("SlackWebHook");
#endif
        }
    }
}
