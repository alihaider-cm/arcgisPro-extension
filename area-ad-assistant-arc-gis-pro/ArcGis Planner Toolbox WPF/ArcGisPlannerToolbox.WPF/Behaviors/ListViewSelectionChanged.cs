using ArcGisPlannerToolbox.Core.Models.DTO;
using Microsoft.Xaml.Behaviors;
using System.Windows.Controls;

namespace ArcGisPlannerToolbox.WPF.Behaviors;

public class ListViewSelectionChanged : Behavior<ListView>
{
    protected override void OnAttached()
    {
        AssociatedObject.SelectionChanged += AssociatedObject_SelectionChanged;
    }
    protected override void OnDetaching()
    {
        AssociatedObject.SelectionChanged -= AssociatedObject_SelectionChanged;
    }
    private void AssociatedObject_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems.Count > 0)
            foreach (OccupancyUnitDTO occupancyUnit in e.AddedItems)
                occupancyUnit.IsChecked = true;
        else if (e.RemovedItems.Count > 0)
            foreach (OccupancyUnitDTO occupancyUnit in e.RemovedItems)
                occupancyUnit.IsChecked = false;
    }
}
