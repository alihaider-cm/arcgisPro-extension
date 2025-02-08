using System.Collections.Generic;

namespace ArcGisPlannerToolbox.Core.Models;

public class ColorPalette
{
    public ColorPalette()
    {
    }

    public Color Name { get; set; }

    public List<string> Colors { get; set; }
}
