using ArcGIS.Core.Events;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Framework;
using ArcGisPlannerToolbox.Core.Contracts;
using ArcGisPlannerToolbox.Core.Models;
using ArcGisPlannerToolbox.WPF.Helpers;
using ArcGisPlannerToolbox.WPF.Helpers.Excel;
using ArcGisPlannerToolbox.WPF.Services;
using Microsoft.Win32;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using MultiBranchWizardSteps = ArcGisPlannerToolbox.WPF.Events.MultiBranchPlanAdvertisementAreaWizardStepsCompleted;

namespace ArcGisPlannerToolbox.WPF.ViewModels;

public class ParticipatingBranchesView2ViewModel : WizardPageViewModel
{
    private readonly IPlanningRepository _planningRepository;
    private readonly ICursorService _cursorService;
    private readonly SubscriptionToken _customerChangedSubscriptionToken;


    private string _filePath = string.Empty;
    public string FilePath
    {
        get { return _filePath; }
        set { _filePath = value; OnPropertyChanged(); }
    }

    private List<CustomerBranch> _data = new();
    public List<CustomerBranch> Data
    {
        get { return _data; }
        set { _data = value; OnPropertyChanged(); }
    }

    private bool _canUpdateDatabase = false;
    public bool CanUpdateDatabase
    {
        get { return _canUpdateDatabase; }
        set { _canUpdateDatabase = value; OnPropertyChanged(); }
    }

    private bool _useLatestBranches;
    public bool UseLatestBranches
    {
        get { return _useLatestBranches; }
        set { _useLatestBranches = value; OnPropertyChanged(); OnCheckBoxStatusChanged(); }
    }

    private double _progress = 0;
    public double Progress
    {
        get { return _progress; }
        set { _progress = value; OnPropertyChanged(); }
    }

    private string _uploadStatus = "Lade Filialdaten in die Datenbank ...";
    public string UploadStatus
    {
        get { return _uploadStatus; }
        set { _uploadStatus = value; OnPropertyChanged(); }
    }


    public int SelectedCustomerId { get; set; }
    public ICommand FileDialogCommand { get; set; }
    public ICommand ReadFileCommand { get; set; }
    public ICommand UpdateDatabaseCommand { get; set; }

    public ParticipatingBranchesView2ViewModel(IPlanningRepository planningRepository, ICursorService cursorService)
    {
        _planningRepository = planningRepository;
        _cursorService = cursorService;

        FileDialogCommand = new RelayCommand(OpenDialog);
        ReadFileCommand = new RelayCommand(ProcessFile);
        UpdateDatabaseCommand = new RelayCommand(OnUpdateDatabase);

        _customerChangedSubscriptionToken = MultiBranchWizardSteps.CustomerChanged.Subscribe(x => SelectedCustomerId = x.Kunden_ID);
    }

    public void OnWindowLoaded()
    {
        Heading = "Teilnehmende Filialen";
        OnCheckBoxStatusChanged();
    }
    private async Task ProcessFile()
    {
        if (!string.IsNullOrWhiteSpace(FilePath) && File.Exists(FilePath))
        {
            ProApp.Current.MainWindow.Cursor = Cursors.Wait;
            _cursorService.SetCursor(Cursors.Wait);
            Data = await Task.Run<List<CustomerBranch>>(() => GetBranchData(FilePath));
            CanUpdateDatabase = true;
            ProApp.Current.MainWindow.Cursor = Cursors.Arrow;
            _cursorService.SetCursor(Cursors.Arrow);
        }
        else
            MessageBox.Show("File doesn't exist", "Incorrect Path", MessageBoxButton.OK, MessageBoxImage.Error);
    }
    private void OpenDialog()
    {
        FilePath = GetFilePathFromDialog();
    }
    private string GetFilePathFromDialog()
    {
        OpenFileDialog openFileDialog = new OpenFileDialog();
        openFileDialog.InitialDirectory = "c:\\";
        openFileDialog.Filter = "Excel Files| *.xls; *.xlsx; *.xlsm";
        openFileDialog.FilterIndex = 2;
        openFileDialog.RestoreDirectory = true;

        if (openFileDialog.ShowDialog() == true)
            return openFileDialog.FileName;

        return string.Empty;
    }
    private List<CustomerBranch> GetBranchData(string filePath)
    {
        try
        {
            //using (ExcelHelper excelHelper = new SyncfusionExcel())
            //{
            //    var tableData = excelHelper.GetDataFromFile(filePath);
            //    return tableData.ToList<CustomerBranch>();
            //}
            return ReadExcelTable(filePath, "Tabelle1");
        }
        catch (IOException e)
        {
            MessageBox.Show(e.Message, "Probleme beim Öffnen einer Datei", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        catch (ArgumentException)
        {
            MessageBox.Show("Es wurde keine Datei gewählt.", "Probleme beim Öffnen einer Datei", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        finally
        {
        }
        return Enumerable.Empty<CustomerBranch>().ToList();
    }
    private List<CustomerBranch> ReadExcelTable(string filePath, string tableName)
    {
        List<CustomerBranch> entries = new List<CustomerBranch>();

        // Initialisieren Sie die Excel-Engine
        using (ExcelEngine excelEngine = new ExcelEngine())
        {
            IApplication application = excelEngine.Excel;
            application.DefaultVersion = ExcelVersion.Excel2016;

            // Laden Sie die Excel-Datei
            using (FileStream inputStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                IWorkbook workbook = application.Workbooks.Open(inputStream);

                // Holen Sie sich das Arbeitsblatt
                IWorksheet worksheet = workbook.Worksheets[0];

                DataTable dataTable = worksheet.ExportDataTable(worksheet.UsedRange, ExcelExportDataTableOptions.ColumnNames);

                // Durchlaufen Sie die Zeilen der DataTable
                foreach (DataRow row in dataTable.Rows)
                {
                    if(int.TryParse(row["Auflage"].ToString(), out int copies))
                    {
                        CustomerBranch entry = new CustomerBranch
                        {
                            Filial_Nr = row["Filial_Nr"].ToString(),
                            Auflage = copies
                        };
                        entries.Add(entry);
                    } else
                    {
                        break;
                    }
                    
                }
            }
        }

        return entries;
    }
    private async Task OnUpdateDatabase() => await OnExecuteUpload();
    private async Task OnExecuteUpload()
    {
        try
        {
            Progress = 0;
            ProApp.Current.MainWindow.Cursor = Cursors.Wait;
            _cursorService.SetCursor(Cursors.Wait);
            var uniqueBranches = Data.Distinct().ToList();
            int currentItem = 1;
            await _planningRepository.DeleteFormerBranchSelectionAsync(SelectedCustomerId);
            foreach (var branch in uniqueBranches)
            {
                await _planningRepository.ExecuteUploadBranchDataToDatabase(SelectedCustomerId, branch);
                Progress = CalculateProgress(currentItem, uniqueBranches.Count);
                currentItem++;
            }
            ProApp.Current.MainWindow.Cursor = Cursors.Arrow;
            _cursorService.SetCursor(Cursors.Arrow);
            UploadStatus = "Fertig!";
        }
        catch (Exception ex)
        {
            ProApp.Current.MainWindow.Cursor = Cursors.Arrow;
            _cursorService.SetCursor(Cursors.Arrow);
        }
        AllowNext = true;

    }
    private double CalculateProgress(int current, int total)
    {
        double value = (current * 100d) / total;
        return Math.Round(value, 2);
    }
    private void OnCheckBoxStatusChanged()
    {
        if (UseLatestBranches)
            AllowNext = true;

        NextStep = 6;
    }
}
