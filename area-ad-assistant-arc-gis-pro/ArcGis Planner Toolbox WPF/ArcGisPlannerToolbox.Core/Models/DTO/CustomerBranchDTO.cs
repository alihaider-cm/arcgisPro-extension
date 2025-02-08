namespace ArcGisPlannerToolbox.Core.Models.DTO;

public class CustomerBranchDTO : CustomerBranch
{
    private bool _isChecked;
    public bool IsChecked
    {
        get { return _isChecked; }
        set { _isChecked = value; OnPropertyChanged(); }
    }
    public CustomerBranchDTO()
    {

    }
    public CustomerBranchDTO(CustomerBranch customerBranch)
    {
        Filial_ID = customerBranch.Filial_ID;
        Filial_Nr = customerBranch.Filial_Nr;
        Filialname = customerBranch.Filialname;
        Auflage = customerBranch.Auflage;
        Entfernung_in_km = customerBranch.Entfernung_in_km;
        Kundenname = customerBranch.Kundenname;
        Kunden_ID = customerBranch.Kunden_ID;
        Medium_Id = customerBranch.Medium_Id;
        ORT = customerBranch.ORT;
        PLZ = customerBranch.PLZ;
        Straße = customerBranch.Straße;
        Vertriebslinie = customerBranch.Vertriebslinie;
        VonAlgorithmusGeplant = customerBranch.VonAlgorithmusGeplant;
        X_WGS84 = customerBranch.X_WGS84;
        Y_WGS84 = customerBranch.Y_WGS84;
        IsChecked = false;
    }
}
