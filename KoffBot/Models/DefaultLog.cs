namespace KoffBot.Models;

public class DefaultLog
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string ModifiedBy { get; set; } = string.Empty;
    public DateTime Modified { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime Created { get; set; } = DateTime.UtcNow;
}
