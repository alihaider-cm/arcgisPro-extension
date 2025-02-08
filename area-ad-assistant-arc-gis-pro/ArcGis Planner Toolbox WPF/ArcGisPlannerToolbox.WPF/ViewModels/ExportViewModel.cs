using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Internal.Catalog;
using ArcGisPlannerToolbox.Core.Contracts;
using ArcGisPlannerToolbox.Core.Models;
using ArcGisPlannerToolbox.WPF.Helpers;
using ArcGisPlannerToolbox.WPF.Helpers.Excel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;

namespace ArcGisPlannerToolbox.WPF.ViewModels;

public class ExportViewModel : WizardPageViewModel
{
    private readonly IPlanningRepository _planningRepository;

    public ICommand ExportCommand { get; set; }
    public ExportViewModel(IPlanningRepository planningRepository)
    {
        _planningRepository = planningRepository;
        ExportCommand = new RelayCommand(OnExport);
    }
    public void OnLoaded()
    {
        Heading = "Speichern / Export";
        IsLastPage = true;
    }
    private void OnExport()
    {
        var planningResults = _planningRepository.GetPlanningResult();
        var planningResultDetails = _planningRepository.GetPlanningResultDetails();
        if (!(planningResults.Count > 0 && planningResultDetails.Count > 0))
        {
            System.Windows.MessageBox.Show("Keine Planungsergebnisse gefunden!",
                "Excel Export", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }
        string folderPath = GetFilePathFromFolderDialog();
        if (folderPath == "") return;

        ExportDigitisationStatesToExcel(folderPath, planningResults, planningResultDetails);
        System.Windows.MessageBox.Show($"Das Planungsergebnis wurde erfolgreich nach {folderPath} exportiert.",
                "Excel Export", MessageBoxButton.OK, MessageBoxImage.Information);
    }
    private void ExportDigitisationStatesToExcel(string folderPath, List<PlanningResult> planningResults, List<PlanningResultDetails> planningResultDetails)
    {
        try
        {
            using (ExcelHelper exportHelper = new SyncfusionExcel())
            {
                DateTime now = DateTime.Now;
                string savePath = $"{folderPath}\\Gebietsassistent_Planungsergebnis_{now.ToShortDateString()}_{now.Hour}{now.Minute}.xlsx";
                exportHelper.CreateWorksheet("Toureninfo", planningResults);
                exportHelper.CreateWorksheet("mPLZ", planningResultDetails);
                exportHelper.Save(savePath);
            }
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

    }
    private string GetFilePathFromFolderDialog()
    {
        var directoryPath = string.Empty;

        using (FolderBrowserDialog openFolderDialog = new FolderBrowserDialog())
        {
            openFolderDialog.ShowNewFolderButton = true;

            if (openFolderDialog.ShowDialog() != DialogResult.OK)
            {
                return string.Empty;
            }
            else
            {
                //Get the path of specified file
                directoryPath = openFolderDialog.SelectedPath;

                return directoryPath;
            }
        }
    }
}
