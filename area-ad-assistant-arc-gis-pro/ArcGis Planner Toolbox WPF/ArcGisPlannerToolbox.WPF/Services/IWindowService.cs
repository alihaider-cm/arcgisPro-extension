using ArcGIS.Desktop.Framework.Controls;

namespace ArcGisPlannerToolbox.WPF.Services;

public interface IWindowService
{
    void ShowWindow<T>() where T : ProWindow;
    void CloseWindow<T>() where T : ProWindow;
}
