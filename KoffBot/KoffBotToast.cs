using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace KoffBot;

public class KoffBotToast
{
    private readonly ILogger _logger;

    public KoffBotToast(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<KoffBotToast>();
    }

    [Function("KoffBotToast")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequestData req)
    {
        _logger.LogInformation("KoffBot activated. Ready for furious toasting.");
        var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", EnvironmentVariableTarget.Process);
        if (env != Shared.LocalEnvironmentName)
        {
            await AuthenticationService.Authenticate(req, _logger);
        }

        // Check if in "Drunk Mode".
        var drunkMode = false;
        try
        {
            var connectionString = Environment.GetEnvironmentVariable("DbConnectionString");
            using SqlConnection conn = new SqlConnection(connectionString);

            conn.Open();
            var sqlGet = $@"SELECT TOP 1 * FROM LogDrunk ORDER BY id DESC";

            using SqlCommand cmd = new SqlCommand(sqlGet, conn);
            var rows = await cmd.ExecuteReaderAsync();
            var lastDate = new DateTime();
            while (await rows.ReadAsync())
            {
                lastDate = Convert.ToDateTime(rows[4]);
            }

            rows.Close();

            if (DateTime.UtcNow < lastDate.AddHours(1))
            {
                drunkMode = true;
            }
        }
        catch (Exception e)
        {
            _logger.LogError("Reading from drunkedness log failed.", e);
            var result = req.CreateResponse(HttpStatusCode.OK);
            result.WriteString("Reading from drunkedness log failed.");

            return result;
        }

        // Send message to Slack channel.
        var message = new ToastSlackMessageDTO()
        {
            Text = "Koff! Nyt GitHub Actioneilla!!!"
        };

        if (drunkMode)
        {
            message.Text = ScrambleWord(message.Text);
        }

        using var httpClient = new HttpClient();
        var content = new HttpRequestMessage(HttpMethod.Post, Shared.GetResponseEndpoint())
        {
            Content = new StringContent(JsonSerializer.Serialize(message), Encoding.UTF8, "application/json")
        };
        await httpClient.SendAsync(content);

        // Log the toasting.
        try
        {
            var connectionString = Environment.GetEnvironmentVariable("DbConnectionString");
            using SqlConnection conn = new SqlConnection(connectionString);

            conn.Open();
            var sql = $@"INSERT INTO LogToast (Created, CreatedBy, Modified, ModifiedBy)
                             VALUES (CURRENT_TIMESTAMP, 'KoffBotToast', CURRENT_TIMESTAMP, 'KoffBotToast')";

            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                await cmd.ExecuteNonQueryAsync();
            }
        }
        catch (Exception e)
        {
            _logger.LogError("Saving into toasting log failed.", e);
            var result = req.CreateResponse(HttpStatusCode.InternalServerError);
            result.WriteString("Saving into toasting log failed.");

            return result;
        }

        return req.CreateResponse(HttpStatusCode.OK);
    }

    private static string ScrambleWord(string str)
    {
        var rand = new Random(); 
        var list = new SortedList<int, char>();
        foreach (var c in str)
            list.Add(rand.Next(), c);
        
        return new string(list.Values.ToArray());
    }
}