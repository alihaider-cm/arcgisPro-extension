using ArcGisPlannerToolbox.Core.Models;
using ArcGisPlannerToolbox.WPF.Repositories.Contracts;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using MultiBranchWizardSteps = ArcGisPlannerToolbox.WPF.Events.MultiBranchPlanAdvertisementAreaWizardStepsCompleted;

namespace ArcGisPlannerToolbox.WPF.ViewModels;

public class CustomerDataViewModel : WizardPageViewModel
{
    #region Fields

    private readonly ICustomerRepository _customerRepository;

    #endregion

    #region Properties

    public ICommand RadioSelectionCommand { get; set; }

    private List<Customer> _customers = new();
    public List<Customer> Customers
    {
        get { return _customers; }
        set { _customers = value; OnPropertyChanged(); }
    }

    private List<string> _customerNames = new();
    public List<string> CustomerNames
    {
        get { return _customerNames; }
        set { _customerNames = value; OnPropertyChanged(); }
    }

    private string _selectedCustomerName = string.Empty;
    public string SelectedCustomerName
    {
        get { return _selectedCustomerName; }
        set { _selectedCustomerName = value; OnPropertyChanged(); EnableNextButton(); }
    }

    private string _selectedOption = string.Empty;
    public string SelectedOption
    {
        get { return _selectedOption; }
        set { _selectedOption = value; OnPropertyChanged(); EnableNextButton(); }
    }


    #endregion

    #region Initialization

    public CustomerDataViewModel(ICustomerRepository customerRepository)
    {
        
        _customerRepository = customerRepository;
        RadioSelectionCommand = new RelayCommand<string>((x) => SelectedOption = x);
    }

    #endregion

    #region Public Methods

    public void OnWindowLoaded()
    {
        Heading = "Kundendaten";
        GetCustomers();
        EnableNextButton();
    }

    #endregion

    #region Private Methods

    private void GetCustomers()
    {
        Customers = _customerRepository.GetCustomers().OrderBy(c => c.Kunde).ToList();
        CustomerNames = Customers.Select(x => x.Kunde).ToList();
    }

    private void EnableNextButton()
    {
        if (!string.IsNullOrEmpty(SelectedOption) && !string.IsNullOrEmpty(SelectedCustomerName))
        {
            //var customerId = GetCustomerId(SelectedCustomerName);
            var customer = GetCustomer(SelectedCustomerName);
            MultiBranchWizardSteps.CustomerChanged.Publish(customer);
            MultiBranchWizardSteps.FormerPlanningChanged.Publish(SelectedOption);
            NextStep = GetNextPageNumber();
            AllowNext = true;
        }
    }
    private int GetNextPageNumber() => SelectedOption switch
    {
        "All" => 2,
        "SelectFromList" => 3,
        "ImportList" => 4,
        "OpenFormerPlanning" => 5,
        _ => 0
    };
    private int GetCustomerId(string customerName)
    {
        var customer = Customers.FirstOrDefault(x => x.Kunde.Equals(customerName, System.StringComparison.OrdinalIgnoreCase));
        if (customer is not null)
            return customer.Kunden_ID;

        return -1;
    }
    private Customer GetCustomer(string customerName) => Customers.FirstOrDefault(x => x.Kunde.Equals(customerName, System.StringComparison.OrdinalIgnoreCase));

    #endregion
}
