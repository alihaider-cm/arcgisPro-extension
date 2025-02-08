using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Controls;
using ArcGisPlannerToolbox.WPF.Startup;
using Autofac;
using System;
using System.Collections.Generic;

namespace ArcGisPlannerToolbox.WPF.Services;

public class WindowService : IWindowService
{
    private Dictionary<string, ProWindow> _windowCollection = new();
    public void ShowWindow<T>() where T : ProWindow
    {
        var window = CreateWindow<T>();
        if (_windowCollection.TryGetValue(window.Title.Trim(), out ProWindow oldWindow))
        {
            if (oldWindow.WindowState == System.Windows.WindowState.Minimized)
                oldWindow.WindowState = System.Windows.WindowState.Normal;
            oldWindow.Activate();
        }
        else
        {
            _windowCollection.Add(window.Title.Trim(), window);
            window.Owner = FrameworkApplication.Current.MainWindow;
            window.Closed += Window_Closed;
            window.Show();
        }
    }

    private void Window_Closed(object sender, EventArgs e)
    {
        var window = sender as ProWindow;
        _windowCollection.Remove(window.Title.Trim());
    }

    private ProWindow CreateWindow<T>() where T : ProWindow
    {
        var window = App.Container.Resolve<T>();
        window.Topmost = true;
        if (window is null)
            throw new InvalidOperationException($"Could not create window for {typeof(T)}");

        return window;
    }

    public void CloseWindow<T>() where T : ProWindow
    {
        var window = CreateWindow<T>();
        if (_windowCollection.TryGetValue(window.Title.Trim(), out ProWindow oldWindow))
            oldWindow.Close();
    }
}
