using ArcGIS.Core.Events;
using ArcGIS.Desktop.Framework;
using ArcGisPlannerToolbox.WPF.Data;
using ArcGisPlannerToolbox.WPF.Services;
using ArcGisPlannerToolbox.WPF.Views;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using static ArcGisPlannerToolbox.WPF.Events.PlanAdvertisementAreaWizardEvent;

namespace ArcGisPlannerToolbox.WPF.ViewModels;

public class PlanningLayerSelectionWindowViewModel : BindableBase
{
    #region Fields

    private SubscriptionToken subscriptionToken;
    private readonly IWindowService _windowService;

    #endregion

    #region Properties

    private List<PolygonGeometry> _groupData = new();
    public List<PolygonGeometry> GroupData
    {
        get { return _groupData; }
        set { _groupData = value; OnPropertyChanged(); }
    }

    private PolygonGeometry _selectedGroupData = new();
    public PolygonGeometry SelectedGroupData
    {
        get { return _selectedGroupData; }
        set { _selectedGroupData = value; OnPropertyChanged(); }
    }

    public ICommand SubmitButton { get; set; }

    #endregion

    #region Init

    public PlanningLayerSelectionWindowViewModel(IWindowService windowService)
    {
        _windowService = windowService;
        SubmitButton = new RelayCommand(OnSubmit);
    }

    #endregion

    #region Public Methods

    public void OnWindowLoaded()
    {
        subscriptionToken = PlanningLayerSelectionChanged.Subscribe(OnSelectionChanged);
    }

    #endregion

    #region Private Methods

    private void OnSelectionChanged(List<PolygonGeometry> selectedFeatures)
    {
        GroupData = selectedFeatures;

    }
    private void OnSubmit()
    {
        if (SelectedGroupData is not null)
        {
            PlanningLayerSelectedItemChanged.Publish(SelectedGroupData);
            PlanningLayerSelectionChanged.Unsubscribe(subscriptionToken);
            _windowService.CloseWindow<PlanningLayerSelectionWindow>();
        }
        else
            MessageBox.Show("Please made a selection", "Invalid Operation", MessageBoxButton.OK, MessageBoxImage.Error);
    }

    #endregion

}
