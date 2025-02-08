using ArcGisPlannerToolbox.WPF.ViewModels;
using System;
using System.Globalization;
using System.Windows.Data;

namespace ArcGisPlannerToolbox.WPF.Converters;

public class StepToContentConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values[0] is int stepIndex && values[1] is WizardControlViewModel viewModel)
            if (stepIndex >= 0 && stepIndex < viewModel.Steps.Count)
                return viewModel.Steps[stepIndex];

        return null;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}