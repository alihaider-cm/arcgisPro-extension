using ArcGisPlannerToolbox.Core.Models;
using ArcGisPlannerToolbox.WPF.Repositories.Contracts;
using System.Collections.Generic;
using System.Linq;
using static ArcGisPlannerToolbox.WPF.Events.PlanAdvertisementAreaWizardEvent;

namespace ArcGisPlannerToolbox.WPF.ViewModels;

public class CustomerViewModel : WizardPageViewModel
{
    #region Fields

    private readonly ICustomerRepository _customerRepository;
    private List<Customer> _customers = new();
    private List<CustomerBranch> _branches = new();

    #endregion

    #region Properties

    private List<Customer> customers = new();
    public List<Customer> Customers
    {
        get { return customers; }
        set { customers = value; OnPropertyChanged(); }
    }

    private Customer _selectedCustomer;
    public Customer SelectedCustomer
    {
        get { return _selectedCustomer; }
        set { _selectedCustomer = value; OnPropertyChanged(); }
    }

    private List<CustomerBranch> _customerBranches = new();
    public List<CustomerBranch> CustomerBranches
    {
        get { return _customerBranches; }
        set { _customerBranches = value; OnPropertyChanged(); }
    }

    private CustomerBranch _selectedBranch;
    public CustomerBranch SelectedBranch
    {
        get { return _selectedBranch; }
        set { _selectedBranch = value; OnPropertyChanged(); }
    }

    private string _searchText = string.Empty;
    public string SearchText
    {
        get { return _searchText; }
        set { _searchText = value; OnPropertyChanged(); }
    }

    #endregion

    #region Constructor

    public CustomerViewModel(ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;
    }

    #endregion

    #region Public Methods

    public void OnLoaded()
    {
        Heading = "Kunde";
        NextStep = 2;
        if (_customers.Count == 0)
            Customers = _customers = _customerRepository.GetCustomers().OrderBy(c => c.Kunde).ToList();

        if (SelectedCustomer is not null && SelectedBranch is not null)
            AllowNext = true;
    }

    public void OnTextChanged()
    {
        CustomerBranches = GetFilteredBranches();
    }

    public void OnCustomerChanged()
    {
        SelectedCustomerChanged.Publish(SelectedCustomer);
        _branches = CustomerBranches = _customerRepository.GetBranchByCustomer(SelectedCustomer.Kunden_ID);
    }

    public void OnCustomerBranchChanged()
    {
        CustomerBranchChanged.Publish(SelectedBranch);
        AllowNext = true;
    }

    #endregion

    #region Private Methods

    private List<CustomerBranch> GetFilteredBranches()
    {
        if (SearchText.Length > 0)
        {
            return _branches
                .Where
                (
                    b => b.Filial_Nr == null || b.Filial_Nr.Contains(SearchText)
                    || (b.Filialname == null || b.Filialname.ToLower().Contains(SearchText)
                    || (b.ORT == null || b.ORT.ToLower().Contains(SearchText)
                    || (b.Straße == null || b.Straße.ToLower().Contains(SearchText)
                    || (b.PLZ == null || b.PLZ.ToLower().Contains(SearchText)))))
                )
                .ToList();
        }
        return _branches;
    }

    #endregion

}
