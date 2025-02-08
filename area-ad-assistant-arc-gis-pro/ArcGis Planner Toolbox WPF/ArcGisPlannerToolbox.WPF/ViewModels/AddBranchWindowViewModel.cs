using ArcGIS.Desktop.Framework;
using ArcGisPlannerToolbox.WPF.Events;
using ArcGisPlannerToolbox.WPF.Services;
using ArcGisPlannerToolbox.WPF.Views;
using System.Windows.Input;

namespace ArcGisPlannerToolbox.WPF.ViewModels;

public class AddBranchWindowViewModel : BindableBase
{
    private readonly IWindowService _windowService;

    private string _filialNr;
    public string FilialNr
    {
        get { return _filialNr; }
        set { _filialNr = value; OnPropertyChanged(); }
    }

    private int _zielAuflage;
    public int ZielAuflage
    {
        get { return _zielAuflage; }
        set { _zielAuflage = value; OnPropertyChanged(); }
    }

    public ICommand SubmitCommand { get; set; }
    public AddBranchWindowViewModel(IWindowService windowService)
    {
        _windowService = windowService;
        SubmitCommand = new RelayCommand(OnSubmit);
    }

    private void OnSubmit()
    {
        BranchCreatedEvent.Publish(new Core.Models.CustomerBranch()
        {
            Filial_Nr = FilialNr,
            Auflage = ZielAuflage,
        });
        _windowService.CloseWindow<AddBranchWindow>();
    }
}
