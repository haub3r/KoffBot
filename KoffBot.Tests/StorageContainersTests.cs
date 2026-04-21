using KoffBot.Models;

namespace KoffBot.Tests;

public class StorageContainersTests
{
    [Theory]
    [InlineData(StorageContainers.LogDrunk, "log-drunk")]
    [InlineData(StorageContainers.LogFriday, "log-friday")]
    [InlineData(StorageContainers.LogPrice, "log-price")]
    [InlineData(StorageContainers.LogToast, "log-toast")]
    [InlineData(StorageContainers.Holidays, "holidays")]
    public void StorageContainer_HasExpectedValue(string actual, string expected)
    {
        Assert.Equal(expected, actual);
    }
}
