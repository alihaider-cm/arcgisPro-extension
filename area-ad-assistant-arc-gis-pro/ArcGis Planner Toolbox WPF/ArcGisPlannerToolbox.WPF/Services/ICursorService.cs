using System;
using System.Windows.Input;

namespace ArcGisPlannerToolbox.WPF.Services;

public interface ICursorService
{
    void SetCursor(Cursor cursor);
    event Action<Cursor> CursorChanged;
}
