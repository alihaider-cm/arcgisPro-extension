using Microsoft.Xaml.Behaviors;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace ArcGisPlannerToolbox.WPF.Behaviors;

public class ListViewGroupingBehavior : Behavior<ListView>
{
    public string GroupBy
    {
        get { return (string)GetValue(GroupByProperty); }
        set { SetValue(GroupByProperty, value); }
    }
    public static readonly DependencyProperty GroupByProperty =
        DependencyProperty.Register(nameof(GroupBy), typeof(string), typeof(ListViewGroupingBehavior), new PropertyMetadata(string.Empty));

    public string SortBy
    {
        get { return (string)GetValue(SortByProperty); }
        set { SetValue(SortByProperty, value); }
    }
    public static readonly DependencyProperty SortByProperty =
        DependencyProperty.Register(nameof(SortBy), typeof(string), typeof(ListViewGroupingBehavior), new PropertyMetadata(string.Empty));

    public List<object> Items
    {
        get { return (List<object>)GetValue(ItemsProperty); }
        set { SetValue(ItemsProperty, value); }
    }
    public static readonly DependencyProperty ItemsProperty =
        DependencyProperty.Register(nameof(Items), typeof(List<object>), typeof(ListViewGroupingBehavior), new FrameworkPropertyMetadata(new List<object>(), OnItemsChanged));

    private static void OnItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ListViewGroupingBehavior listViewGroupingBehavior)
        {
            listViewGroupingBehavior.GroupItems();
        }
    }

    protected override void OnAttached()
    {
        AssociatedObject.Loaded += AssociatedObject_Loaded;
        AssociatedObject.SelectionChanged += AssociatedObject_SelectionChanged;
    }

    private void AssociatedObject_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (AssociatedObject.SelectedIndex == -1)
            GroupItems();
    }
    protected override void OnDetaching()
    {
        AssociatedObject.Loaded -= AssociatedObject_Loaded;
        AssociatedObject.SelectionChanged -= AssociatedObject_SelectionChanged;
    }
    private void AssociatedObject_Loaded(object sender, RoutedEventArgs e) => GroupItems();
    private void GroupItems()
    {
        if (AssociatedObject.ItemsSource is null)
            return;

         CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(AssociatedObject.ItemsSource);
        var groupDescription = new PropertyGroupDescription(GroupBy);
        var sortDescription = new SortDescription(SortBy, ListSortDirection.Ascending);
        
        if (view.GroupDescriptions.Count == 0 && !string.IsNullOrWhiteSpace(GroupBy))
            view.GroupDescriptions.Add(groupDescription);

        if (view.SortDescriptions.Count == 0 && !string.IsNullOrWhiteSpace(SortBy))
            view.SortDescriptions.Add(sortDescription);
    }
}
