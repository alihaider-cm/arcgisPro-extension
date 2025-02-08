using ArcGIS.Core.Events;
using ArcGIS.Desktop.Framework;
using ArcGisPlannerToolbox.Core.Models;
using ArcGisPlannerToolbox.WPF.Events;
using ArcGisPlannerToolbox.WPF.Repositories.Contracts;
using ArcGisPlannerToolbox.WPF.Services;
using ArcGisPlannerToolbox.WPF.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace ArcGisPlannerToolbox.WPF.ViewModels;

public class TourSplitterWindowViewModel : BindableBase
{
    #region Fields

    private readonly IGeometryRepositoryGebietsassistent _geometryRepositoryGebietsassistent;
    private readonly IWindowService _windowService;
    private SubscriptionToken _tourSelectionSubscriptionToken;
    private SubscriptionToken _subTourCreatedSubscriptionToken;
    private List<Tour> _currentAreas = new();
    private Tour _selectedTour = new();
    private char _currentChar;
    private List<Tour> _suggestedSubTours = new();
    private List<Tour> _selectedSubTours = new();
    #endregion

    #region Properties

    public ICommand SplitTourCommand { get; set; }
    public ICommand OpenSubTourWindowCommand { get; set; }
    public ICommand UpdatePrintNumberCommand { get; set; }
    public ICommand MergeToursCommand { get; set; }
    public ICommand RemoveTourCommand { get; set; }
    public ICommand ResetCommand { get; set; }
    public ICommand SubmitCommand { get; set; }

    private string _tourName = string.Empty;
    public string TourName
    {
        get { return _tourName; }
        set { _tourName = value; OnPropertyChanged(); }
    }

    private string _tourPrintNumber = string.Empty;
    public string TourPrintNumber
    {
        get { return _tourPrintNumber; }
        set { _tourPrintNumber = value; OnPropertyChanged(); }
    }

    private List<Tour> _subTours = new();
    public List<Tour> SubTours
    {
        get { return _subTours; }
        set { _subTours = value; OnPropertyChanged(); }
    }

    private Tour _selectedSubTour = new();
    public Tour SelectedSubTour
    {
        get { return _selectedSubTour; }
        set { _selectedSubTour = value; OnPropertyChanged(); }
    }

    private int _updatedPrintNumber = 0;
    public int UpdatedPrintNumber
    {
        get { return _updatedPrintNumber; }
        set { _updatedPrintNumber = value; OnPropertyChanged(); }
    }

    private int _newUpdatedPrintNumber = 0;
    public int NewUpdatedPrintNumber
    {
        get { return _newUpdatedPrintNumber; }
        set { _newUpdatedPrintNumber = value; OnPropertyChanged(); }
    }


    #endregion

    public TourSplitterWindowViewModel(IGeometryRepositoryGebietsassistent geometryRepositoryGebietsassistent,
                                       IWindowService windowService)
    {
        _geometryRepositoryGebietsassistent = geometryRepositoryGebietsassistent;
        _windowService = windowService;
        SplitTourCommand = new RelayCommand(SplitTours);
        OpenSubTourWindowCommand = new RelayCommand(OpenSubTourWindow);
        UpdatePrintNumberCommand = new RelayCommand(OnUpdateSelectedSubTour);
        MergeToursCommand = new RelayCommand(OnMergeTours);
        RemoveTourCommand = new RelayCommand(OnRemoveTour);
        SubmitCommand = new RelayCommand(OnSubmit);
        ResetCommand = new RelayCommand(OnReset);

        _tourSelectionSubscriptionToken = TourListSelectionChanged.Subscribe(OnTourSelectionChanged);
        _subTourCreatedSubscriptionToken = SubTourEventCreatedEvent.Subscribe(OnSubTourCreated);
    }

    #region Public Methods

    public void OnWindowLoaded()
    {

    }
    public void OnWindowClosed()
    {
        TourListSelectionChanged.Unsubscribe(_tourSelectionSubscriptionToken);
        SubTourEventCreatedEvent.Unsubscribe(_subTourCreatedSubscriptionToken);
    }
    //public void OnSubTourSelectionChanged()
    //{
    //    if (SelectedSubTour is not null)
    //    {
    //        UpdatedPrintNumber = SelectedSubTour.PrintNumber;
    //    }
    //}
    public void OnSubTourSelectionChanged(List<object> items)
    {
        _selectedSubTours = items.ConvertAll(x => (Tour)x);
        if (_selectedSubTours is not null)
        {
            UpdatedPrintNumber = _selectedSubTours.Sum(x => x.PrintNumber);
        }
    }

    #endregion

    #region Private Methods

    private void OpenSubTourWindow()
    {
        _windowService.ShowWindow<SubTourWindow>();
    }
    private void OnTourSelectionChanged(Tuple<Tour, List<Tour>> tuple)
    {
        _selectedTour = tuple.Item1;
        _currentAreas = tuple.Item2;

        TourName = _selectedTour?.TourName;
        TourPrintNumber = _currentAreas?.Sum(x => x.PrintNumber).ToString();
    }
    private void SplitTours()
    {
        var subTourNames = CreateSubTourNames(_selectedTour.TourName);
        SubTours = _suggestedSubTours = CreateSubTours(subTourNames);
        TourPrintNumber = SubTours.Sum(x => x.PrintNumber).ToString();
        //_selectedSubTours.Clear();
    }
    private string CreateSubTourName(string tourName)
    {
        string subTourName = "";
        char c;

        bool countedUp = false;
        while (!countedUp)
        {
            if (_subTours.Count > 0)
            {
                c = _currentChar;
                c++;
            }
            else
                c = 'a';
            _currentChar = c;
            countedUp = true;
            subTourName += $"{tourName} (S&G_{c})";
        }
        return subTourName;
    }
    private List<string> CreateSubTourNames(string tourName)
    {
        List<string> subTourNames = new List<string>();
        int counter = 0;
        for (char c = 'a'; c <= 'z'; c++)
        {
            if (counter < _currentAreas.Count)
            {
                var subTourName = $"{tourName} (S&G_{c})";
                subTourNames.Add(subTourName);
                _currentChar = c;
            }
            else
            {
                break;
            }
            counter += 1;
        }
        return subTourNames;
    }
    private List<Tour> CreateSubTours(List<string> subTourNames)
    {
        List<Tour> subTours = new List<Tour>();
        for (int i = 0; i < _currentAreas.Count; i++)
        {
            var subTour = _currentAreas[i];
            subTour.TourName = subTourNames[i];
            subTours.Add(subTour);
        }
        return subTours;
    }
    private void OnSubTourCreated(Tour tour)
    {
        var subTour = InstatiateNewSubTour(tour);
        SubTours.Add(subTour);
        NewUpdatedPrintNumber = SubTours.Sum(x => x.PrintNumber);
        RaiseListViewUpdate();
    }
    private Tour InstatiateNewSubTour(Tour args)
    {
        CloneTour(_selectedTour, out Tour subTour);
        subTour.TourName = CreateSubTourName(subTour.TourName);
        subTour.Location = args.Location;
        subTour.District = args.District;
        subTour.PrintNumber = args.PrintNumber;
        return subTour;
    }
    private void CloneTour(Tour tour, out Tour clonedTour)
    {
        clonedTour = new Tour();
        clonedTour.Author = tour.Author;
        clonedTour.DistanceToOccupancyUnit = tour.DistanceToOccupancyUnit;
        clonedTour.District = tour.District;
        clonedTour.Issue = tour.Issue;
        clonedTour.Location = tour.Location;
        clonedTour.MediaId = tour.MediaId;
        clonedTour.OccupancyUnitId = tour.OccupancyUnitId;
        clonedTour.PrintNumber = tour.PrintNumber;
        clonedTour.TourId = tour.TourId;
        clonedTour.TourName = tour.TourName;
        clonedTour.TourNumber = tour.TourNumber;
        clonedTour.ZipCode = tour.ZipCode;
    }
    private void OnUpdateSelectedSubTour()
    {
        if (SelectedSubTour is not null)
        {
            SubTours.First(x => x.ZipCode == SelectedSubTour.ZipCode).PrintNumber = UpdatedPrintNumber;
            NewUpdatedPrintNumber = SubTours.Sum(x => x.PrintNumber);
        }
    }
    private void OnMergeTours()
    {
        if (_selectedSubTours.Count > 1)
        {
            foreach (var item in _selectedSubTours)
                SubTours.Remove(item);

            RedefineSubTourNames(SubTours);
            var mergedTourNames = CreateMergedTourName(_selectedSubTours.Count);
            for (int i = 0; i < _selectedSubTours.Count; i++)
            {
                var tour = _selectedSubTours[i];
                tour.TourName = mergedTourNames.First();
                SubTours.Add(tour);
            }
            RaiseListViewUpdate();
        }
    }
    private void OnRemoveTour()
    {
        //SubTours.RemoveAll(x => SubTours.Any(y => y.ZipCode == x.ZipCode));
        SubTours.Remove(SelectedSubTour);
        RaiseListViewUpdate();
    }
    private void RaiseListViewUpdate()
    {
        var subTours = new List<Tour>();
        SubTours.ForEach(x => subTours.Add(x));
        SubTours.Clear();
        SubTours = subTours;
    }
    private List<string> CreateMergedTourName(int numOfMergedTours)
    {
        List<string> tourNames = new List<string>();
        char c;
        int counter = 0;

        for (int i = 0; i < numOfMergedTours; i++)
        {
            if (_subTours.Count > 0 || counter > 0)
            {
                c = _currentChar;
                c++;
            }
            else if (counter == 0)
                c = 'a';
            else
                c = 'a';
            _currentChar = c;
            tourNames.Add($"{_selectedTour.TourName} (S&G_{c})");
            counter += 1;
        }
        return tourNames;
    }
    private List<Tour> RedefineSubTourNames(List<Tour> subTours)
    {
        int counter = 0;
        for (char c = 'a'; c <= 'z'; c++)
        {
            if (counter < subTours.Count)
            {
                var subTour = subTours[counter];
                var subTourName = $"{_selectedTour.TourName} (S&G_{c})";
                subTour.TourName = subTourName;
                _currentChar = c;
            }
            else
            {
                break;
            }
            counter += 1;
        }
        return subTours;
    }
    private void OnReset()
    {
        SubTours.Clear();
        RaiseListViewUpdate();
    }
    private void OnSubmit()
    {
        var userPreference = false;
        if (UpdatedPrintNumber != NewUpdatedPrintNumber)
        {
            var dialogResult = MessageBox.Show("Die Gesamtauflage stimmt nicht mit der Auflage der Ursprungstour überein!", "Auflage stimmt nicht"
               , MessageBoxButton.YesNo, MessageBoxImage.Warning);
            userPreference = dialogResult == MessageBoxResult.Yes ? true : false;
        }
        if (userPreference)
        {

            using (var transaction = _geometryRepositoryGebietsassistent.BeginTransaction())
            {
                try
                {
                    _geometryRepositoryGebietsassistent.InsertSubTours(_selectedTour.OccupancyUnitId, _selectedTour.TourName, SubTours, transaction);
                    _geometryRepositoryGebietsassistent.InsertTourAreaMapping(_selectedTour.TourName, SubTours, transaction);
                    _geometryRepositoryGebietsassistent.InsertAreasWithNewTour(_selectedTour.OccupancyUnitId, _selectedTour.TourName, SubTours, transaction);
                    _geometryRepositoryGebietsassistent.CommitTransaction(transaction);
                }
                catch
                {
                    //using (IDataReader reader = transaction.Connection.ExecuteReader("select 1"))
                    //{
                    //    if (!reader.IsClosed)
                    //        reader.Close();
                    //}
                    _geometryRepositoryGebietsassistent.RollbackTransaction(transaction);
                }
            }
            SubTours.Clear();
            TourSplittedEvent.Publish(true);
            _windowService.CloseWindow<TourSplitterWindow>();
        }
    }

    #endregion

}
