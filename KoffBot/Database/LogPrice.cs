using System;
using System.Collections.Generic;

namespace KoffBot.Database;

public partial class LogPrice
{
    public int Id { get; set; }

    public string Amount { get; set; }

    public string ModifiedBy { get; set; }

    public DateTime Modified { get; set; }

    public string CreatedBy { get; set; }

    public DateTime Created { get; set; }
}
