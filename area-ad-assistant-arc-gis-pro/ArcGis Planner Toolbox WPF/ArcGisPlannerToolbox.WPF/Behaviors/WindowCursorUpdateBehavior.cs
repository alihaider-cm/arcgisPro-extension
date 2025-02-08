using ArcGIS.Desktop.Framework.Controls;
using ArcGisPlannerToolbox.WPF.Services;
using ArcGisPlannerToolbox.WPF.Startup;
using Autofac;
using Microsoft.Xaml.Behaviors;
using System.Windows.Input;

namespace ArcGisPlannerToolbox.WPF.Behaviors;

public class WindowCursorUpdateBehavior : Behavior<ProWindow>
{
    private readonly ICursorService _cursorService;
    public WindowCursorUpdateBehavior()
    {
        _cursorService = App.Container.Resolve<ICursorService>();
    }
    protected override void OnAttached()
    {
        base.OnAttached();
        _cursorService.CursorChanged += OnCursorChanged;
    }

    private void OnCursorChanged(Cursor cursor)
    {
        AssociatedObject.Cursor = cursor;
    }
    protected override void OnDetaching()
    {
        base.OnDetaching();
        _cursorService.CursorChanged -= OnCursorChanged;
    }
}
