using Microsoft.Xaml.Behaviors;
using System.Collections;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace ArcGisPlannerToolbox.WPF.Behaviors
{
    public class ListViewUpdateBehavior : Behavior<ListView>
    {
        private ICollectionView _view;

        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register(
                nameof(ItemsSource),
                typeof(IEnumerable),
                typeof(ListViewUpdateBehavior),
                new PropertyMetadata(null, OnItemsSourceChanged));

        public IEnumerable ItemsSource
        {
            get => (IEnumerable)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ListViewUpdateBehavior behavior)
            {
                behavior.SetupCollectionView();
            }
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.Loaded += OnListViewLoaded;
            
            var descriptor = DependencyPropertyDescriptor.FromProperty(
                FrameworkElement.DataContextProperty, 
                typeof(ListView));
            descriptor.AddValueChanged(AssociatedObject, OnDataContextChanged);
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.Loaded -= OnListViewLoaded;
            
            var descriptor = DependencyPropertyDescriptor.FromProperty(
                FrameworkElement.DataContextProperty, 
                typeof(ListView));
            descriptor.RemoveValueChanged(AssociatedObject, OnDataContextChanged);
        }

        private void OnListViewLoaded(object sender, RoutedEventArgs e)
        {
            SetupCollectionView();
        }

        private void OnDataContextChanged(object sender, System.EventArgs e)
        {
            SetupCollectionView();
        }

        private void SetupCollectionView()
        {
            if (AssociatedObject?.ItemsSource == null) return;

            _view = CollectionViewSource.GetDefaultView(AssociatedObject.ItemsSource);
            if (_view != null)
            {
                _view.Refresh();
                
                if (AssociatedObject.DataContext is INotifyPropertyChanged notifier)
                {
                    notifier.PropertyChanged += (s, e) =>
                    {
                        if (e.PropertyName == "DigitizeUnits")
                        {
                            _view.Refresh();
                        }
                    };
                }
            }
        }
    }
} 