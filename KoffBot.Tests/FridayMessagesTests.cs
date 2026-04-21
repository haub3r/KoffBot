using KoffBot.Messages;

namespace KoffBot.Tests;

public class FridayMessagesTests
{
    [Fact]
    public void NormalFridayPossibilities_HasMessages()
    {
        Assert.NotEmpty(FridayMessages.NormalFridayPossibilities);
    }

    [Fact]
    public void NormalFridayPossibilities_AllMessagesAreNonEmpty()
    {
        foreach (var message in FridayMessages.NormalFridayPossibilities)
        {
            Assert.False(string.IsNullOrWhiteSpace(message));
        }
    }

    [Fact]
    public void NormalFridayPossibilitiesWinter_HasMessages()
    {
        Assert.NotEmpty(FridayMessages.NormalFridayPossibilitiesWinter);
    }

    [Fact]
    public void NormalFridayPossibilitiesSummer_HasMessages()
    {
        Assert.NotEmpty(FridayMessages.NormalFridayPossibilitiesSummer);
    }

    [Fact]
    public void Friday13Possibilities_HasMessages()
    {
        Assert.NotEmpty(FridayMessages.Friday13Possibilities);
    }

    [Fact]
    public void Friday13Possibilities_AllMessagesAreNonEmpty()
    {
        foreach (var message in FridayMessages.Friday13Possibilities)
        {
            Assert.False(string.IsNullOrWhiteSpace(message));
        }
    }
}
