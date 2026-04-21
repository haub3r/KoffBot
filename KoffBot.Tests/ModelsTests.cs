using KoffBot.Models;
using KoffBot.Models.Logs;
using System.Text.Json;

namespace KoffBot.Tests;

public class ModelsTests
{
    [Fact]
    public void DefaultLog_HasNewGuidOnCreation()
    {
        var log = new DefaultLog();
        Assert.False(string.IsNullOrEmpty(log.Id));
        Assert.True(Guid.TryParse(log.Id, out _));
    }

    [Fact]
    public void DefaultLog_HasUtcDates()
    {
        var log = new DefaultLog();
        Assert.Equal(DateTimeKind.Utc, log.Created.Kind);
        Assert.Equal(DateTimeKind.Utc, log.Modified.Kind);
    }

    [Fact]
    public void Stats_DefaultValuesAreZero()
    {
        var stats = new Stats();
        Assert.Equal(0, stats.DrunkCount);
        Assert.Equal(0, stats.FridayCount);
        Assert.Equal(0, stats.ToastCount);
    }

    [Fact]
    public void HolidayApiResponse_DeserializesCorrectly()
    {
        var json = """
        {
            "specialDates": [
                { "date": "01.01.2026", "nameFI": "Uudenvuodenpäivä" },
                { "date": "06.12.2026", "nameFI": "Itsenäisyyspäivä" }
            ]
        }
        """;

        var response = JsonSerializer.Deserialize<HolidayApiResponse>(json);

        Assert.NotNull(response);
        Assert.Equal(2, response.SpecialDates.Count);
        Assert.Equal("01.01.2026", response.SpecialDates[0].Date);
        Assert.Equal("Uudenvuodenpäivä", response.SpecialDates[0].Name);
        Assert.Equal("Itsenäisyyspäivä", response.SpecialDates[1].Name);
    }
}
