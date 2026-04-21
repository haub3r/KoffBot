using KoffBot.Messages;

namespace KoffBot.Tests;

public class HolidayMessagesTests
{
    [Fact]
    public void HolidayPossibilities_ContainsAllExpectedHolidays()
    {
        var expectedHolidays = new[]
        {
            "Uudenvuodenpäivä",
            "Pitkäperjantai",
            "Toinen pääsiäispäivä",
            "Vappu",
            "Helatorsta",
            "Juhannusaatto",
            "KoffBotSyntymäpäivä",
            "Itsenäisyyspäivä",
            "Jouluaatto",
            "Tapaninpäivä"
        };

        foreach (var holiday in expectedHolidays)
        {
            Assert.True(HolidayMessages.HolidayPossibilities.ContainsKey(holiday), $"Missing holiday: {holiday}");
        }
    }

    [Fact]
    public void HolidayPossibilities_AllMessagesAreNonEmpty()
    {
        foreach (var (key, value) in HolidayMessages.HolidayPossibilities)
        {
            Assert.False(string.IsNullOrWhiteSpace(value), $"Holiday '{key}' has empty message.");
        }
    }

    [Fact]
    public void PreCelebrationPossibilities_HasAtLeastOneEntry()
    {
        Assert.NotEmpty(HolidayMessages.PreCelebrationPossibilities);
    }

    [Fact]
    public void PreCelebrationPossibilities_AllMessagesAreNonEmpty()
    {
        foreach (var (key, value) in HolidayMessages.PreCelebrationPossibilities)
        {
            Assert.False(string.IsNullOrWhiteSpace(value), $"Pre-celebration '{key}' has empty message.");
        }
    }

    [Fact]
    public void PreCelebrationPossibilities_KeysMatchHolidayPossibilities()
    {
        foreach (var key in HolidayMessages.PreCelebrationPossibilities.Keys)
        {
            Assert.True(HolidayMessages.HolidayPossibilities.ContainsKey(key),
                $"Pre-celebration key '{key}' has no matching holiday.");
        }
    }
}
