using ArcGIS.Desktop.Framework;
using ArcGisPlannerToolbox.WPF.Events;
using ArcGisPlannerToolbox.WPF.Services;
using ArcGisPlannerToolbox.WPF.Views;
using System;
using System.Windows.Input;

namespace ArcGisPlannerToolbox.WPF.ViewModels;

public class SubTourWindowViewModel : BindableBase
{
    #region Fields

    private readonly IWindowService _windowService;

    #endregion

    #region Properties

    public ICommand SubmitCommand { get; set; }

    private string _ort = string.Empty;
	public string Ort
	{
		get { return _ort; }
		set { _ort = value; OnPropertyChanged(); }
	}
    
    private string _ortsteil = string.Empty;
    public string Ortsteil
    {
        get { return _ortsteil; }
        set { _ortsteil = value; OnPropertyChanged(); }
    }
    
    private int _auflage = 0;
    public int Auflage
    {
        get { return _auflage; }
        set { _auflage = value; OnPropertyChanged(); }
    }

    #endregion

    #region Initialization

    public SubTourWindowViewModel(IWindowService windowService)
    {
        _windowService = windowService;
        SubmitCommand = new RelayCommand(OnSubmitDetails);
    }

    #endregion

    #region Private Methods

    private void OnSubmitDetails()
    {
        SubTourEventCreatedEvent.Publish(new Core.Models.Tour()
        {
            Location = Ort,
            District = Ortsteil,
            PrintNumber = Auflage
        });
        _windowService.CloseWindow<SubTourWindow>();
    }

    #endregion
}
