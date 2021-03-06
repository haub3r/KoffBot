﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace KoffBot
{
    public static class Shared
    {
        public static JsonSerializerOptions JsonDeserializerOptions { get; } = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
        };

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
