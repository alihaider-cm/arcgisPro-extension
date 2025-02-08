using ArcGisPlannerToolbox.Core.Models;
using System.Collections.Generic;

namespace ArcGisPlannerToolbox.WPF.Repositories.Contracts;

public interface ICustomerRepository : IRepository<Customer>
{
    List<CustomerBranch> GetBranchesByBranchNumbers(int customerId, List<string> branchNumbers);
    List<CustomerBranch> GetBranchByCustomer(int customerId);
    List<Customer> GetCustomers();
    List<CustomerBranch> GetNearestCustomerBranches(int customerId, CustomerBranch customerBranch);
}
