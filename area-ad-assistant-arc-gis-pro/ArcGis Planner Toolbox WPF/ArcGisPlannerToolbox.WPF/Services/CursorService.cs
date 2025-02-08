using System;
using System.Windows.Input;

namespace ArcGisPlannerToolbox.WPF.Services;

public class CursorService : ICursorService
{
    public event Action<Cursor> CursorChanged = delegate { };

    public void SetCursor(Cursor cursor)
    {
        CursorChanged(cursor);
    }
}
