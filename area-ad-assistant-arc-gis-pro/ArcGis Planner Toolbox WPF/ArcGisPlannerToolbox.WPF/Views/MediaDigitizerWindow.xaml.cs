using ArcGIS.Desktop.Framework.Controls;
using ArcGisPlannerToolbox.WPF.ViewModels;

namespace ArcGisPlannerToolbox.WPF.Views
{
    public partial class MediaDigitizerWindow : ProWindow
    {
        public MediaDigitizerWindow(MediaDigitizerWindowViewModel viewModel)
        {
            DataContext = viewModel;
            InitializeComponent();
        }
    }
}