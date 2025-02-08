using System;

namespace ArcGisPlannerToolbox.WPF.ViewModels;

public class WizardPageViewModel : BindableBase
{
    private string _heading = string.Empty;
    public string Heading
    {
        get { return _heading; }
        set { _heading = value; OnPropertyChanged(); }
    }

    private bool _allowNext;
    public bool AllowNext
    {
        get { return _allowNext; }
        set { _allowNext = value; OnPropertyChanged(); }
    }

    private int _nextStep;
    public int NextStep
    {
        get { return _nextStep; }
        set { _nextStep = value; OnPropertyChanged(); }
    }

    private bool _isLastPage;
    public bool IsLastPage
    {
        get { return _isLastPage; }
        set { _isLastPage = value; OnPropertyChanged(); }
    }

}
