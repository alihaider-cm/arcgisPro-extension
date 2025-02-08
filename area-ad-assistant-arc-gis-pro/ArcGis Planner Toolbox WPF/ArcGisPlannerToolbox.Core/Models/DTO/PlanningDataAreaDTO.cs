using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ArcGisPlannerToolbox.Core.Models.DTO;

public class PlanningDataAreaDTO : PlanningDataArea, INotifyPropertyChanged
{
    private bool _isChecked;
    public bool IsChecked
    {
        get { return _isChecked; }
        set { _isChecked = value; OnPropertyChanged(); }
    }

    public event PropertyChangedEventHandler PropertyChanged;
    public PlanningDataAreaDTO()
    {
        
    }
    public PlanningDataAreaDTO(PlanningDataArea planningDataArea)
    {
        Filial_Nr = planningDataArea.Filial_Nr;
        Auflage = planningDataArea.Auflage;
        Geokey = planningDataArea.Geokey;
        Geokey_Name = planningDataArea.Geokey_Name;
        Geom = planningDataArea.Geom;
        HH_Brutto = planningDataArea.HH_Brutto;
        HH_Netto = planningDataArea.HH_Netto;
        Planungs_ID = planningDataArea.Planungs_ID;
        Planungs_Nr = planningDataArea.Planungs_Nr;
        IsChecked = false;
    }
    public void OnPropertyChanged([CallerMemberName] string propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
