using ArcGisPlannerToolbox.WPF.Events;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ArcGisPlannerToolbox.WPF.ViewModels;

public class WizardControlViewModel : BindableBase
{
    private Stack<int> _pages = new();
    private int _onNext, _onBack;

    private ObservableCollection<UserControl> _steps;
    public ObservableCollection<UserControl> Steps
    {
        get { return _steps; }
        set { _steps = value; OnPropertyChanged(); OnStepsUpdated(); }
    }

    private int _currentStep;
    public int CurrentStep
    {
        get { return _currentStep; }
        set { _currentStep = value; OnPropertyChanged(); OnCurrentStepUpdated(); }
    }
    private bool _canMoveNext;
    public bool CanMoveNext
    {
        get { return _canMoveNext; }
        set { _canMoveNext = value; OnPropertyChanged(); }
    }
    private bool _canMoveBack;
    public bool CanMoveBack
    {
        get { return _canMoveBack; }
        set { _canMoveBack = value; OnPropertyChanged(); }
    }

    private string _heading = string.Empty;
    public string Heading
    {
        get { return _heading; }
        set { _heading = value; OnPropertyChanged(); }
    }

    private Visibility _closeButtonVisibility = Visibility.Collapsed;
    public Visibility CloseButtonVisibility
    {
        get { return _closeButtonVisibility; }
        set { _closeButtonVisibility = value; OnPropertyChanged(); }
    }


    public ICommand NextCommand { get; private set; }
    public ICommand BackCommand { get; private set; }
    public ICommand CancelCommand { get; private set; }
    public ICommand GoToStepCommand { get; private set; }
    public ICommand CloseCommand { get; private set; }

    public WizardControlViewModel()
    {
        Steps = new ObservableCollection<UserControl>();
        NextCommand = new RelayCommand(OnNext);
        BackCommand = new RelayCommand(OnBack);
        CancelCommand = new RelayCommand(OnCancel);
        GoToStepCommand = new RelayCommand<object>(OnGoToStep);
        CloseCommand = new RelayCommand(OnClose);
    }
    private void OnCurrentStepUpdated()
    {
        CanMoveBack = UpdateMoveBack();
    }
    private void OnStepsUpdated()
    {
        CurrentStep = 0;
        foreach (var viewModel in Steps.Select(step => step.DataContext))
        {
            if (viewModel is WizardPageViewModel wizardPageViewModel)
            {
                //wizardPageViewModel.BeforePageChangedEvent += WizardPageViewModel_BeforePageChangedEvent;
                wizardPageViewModel.PropertyChanged += (sender, e) =>
                {
                    if (viewModel is WizardPageViewModel wizardPageViewModel)
                    {
                        switch (e.PropertyName)
                        {
                            case "AllowNext":
                                CanMoveNext = wizardPageViewModel.AllowNext;
                                _onNext = wizardPageViewModel.NextStep;
                                OnCurrentStepUpdated();
                                break;
                            case "IsLastPage":
                                if (wizardPageViewModel.IsLastPage)
                                {
                                    CanMoveNext = false;
                                    CloseButtonVisibility = Visibility.Visible;
                                }
                                break;
                            case "Heading":
                                Heading = wizardPageViewModel.Heading;
                                break;
                        }
                    }
                };
            }

            //if (viewModel is INotifyPropertyChanged notifyPropertyChanged)
            //{
            //    notifyPropertyChanged.PropertyChanged += (sender, e) =>
            //    {
            //        if (viewModel is WizardPageViewModel wizardPageViewModel)
            //        {
            //            switch (e.PropertyName)
            //            {
            //                case "AllowNext":
            //                    CanMoveNext = wizardPageViewModel.AllowNext;
            //                    _onNext = wizardPageViewModel.NextStep;
            //                    OnCurrentStepUpdated();
            //                    break;
            //                case "IsLastPage":
            //                    if (wizardPageViewModel.IsLastPage)
            //                    {
            //                        CanMoveBack = CanMoveNext = false;
            //                        CloseButtonVisibility = Visibility.Visible;
            //                    }
            //                    break;
            //                case "Heading":
            //                    Heading = wizardPageViewModel.Heading;
            //                    break;
            //            }
            //        }
            //    };
            //}
        }
    }
    private void OnNext()
    {
        _pages.Push(CurrentStep);
        CurrentStep = _onNext - 1;
        WizardPageChangedEvent.Publish(true);
        CanMoveNext = false;
    }
    private bool UpdateMoveBack() => CurrentStep > 0;
    private void OnBack()
    {
        _onBack = _pages.First();
        CurrentStep = _onBack;
        //CurrentStep = _onBack - 1;
        _pages.Pop();
    }
    private void OnCancel()
    {
        WizardClosedEvent.Publish(true);
    }
    private void OnGoToStep(object stepIndex)
    {
        if (stepIndex is int validStep && validStep >= 0 && validStep < Steps.Count)
        {
            CurrentStep = validStep;
        }
    }
    private void OnClose()
    {
        WizardClosedEvent.Publish(true);
    }
}
