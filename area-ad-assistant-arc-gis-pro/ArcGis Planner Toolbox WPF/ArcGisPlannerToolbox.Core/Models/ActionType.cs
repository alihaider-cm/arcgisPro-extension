using System;

namespace ArcGisPlannerToolbox.Core.Models;

public class ActionType
{
    public string ActionTypeName { get; set; }
    public int Active { get; set; }
    public string ChangedBy { get; set; }
    public DateTime LastChanged { get; set; }
}
