using System;

namespace KoffBot.Database;

public partial class LogToast
{
    public int Id { get; set; }

    public string ModifiedBy { get; set; }

    public DateTime Modified { get; set; }

    public string CreatedBy { get; set; }

    public DateTime Created { get; set; }
}
