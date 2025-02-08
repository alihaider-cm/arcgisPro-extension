using ArcGisPlannerToolbox.Core.Models;
using Syncfusion.XlsIO;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;

namespace ArcGisPlannerToolbox.WPF.Helpers.Excel;

public class SyncfusionExcel : ExcelHelper
{
    private IWorksheet _worksheet;
    private IWorkbook _workbook;
    private readonly ExcelEngine _excelEngine;
    private readonly IApplication _application;

    public SyncfusionExcel()
    {
        _excelEngine = new ExcelEngine();
        _application = _excelEngine.Excel;
        _application.DefaultVersion = ExcelVersion.Xlsx;
        _workbook = _application.Workbooks.Create(0);
    }

    public override DataTable CreateDataTable<T>(List<T> data)
    {
        DataTable table = new DataTable();
        if (data is not null && data.Count > 0)
        {
            // create header
            foreach (var property in data[0].GetType().GetProperties())
                table.Columns.Add(property.Name, typeof(string));

            // create rows
            foreach (T item in data)
            {
                var row = table.Rows.Add();
                foreach (var property in item.GetType().GetProperties())
                    row[property.Name] = property.GetValue(item)?.ToString();
            }
        }
        ITableStyles tableStyles = _workbook.TableStyles;
        if (!tableStyles.Contains("CustomTableStyle1"))
        {
            ITableStyle tableStyle = tableStyles.Add("CustomTableStyle1");
            ITableStyleElements tableStyleElements = tableStyle.TableStyleElements;
            ITableStyleElement tableStyleElement = tableStyleElements.Add(ExcelTableStyleElementType.SecondColumnStripe);
            tableStyleElement.BackColorRGB = System.Drawing.Color.FromArgb(217, 225, 242);

            ITableStyleElement tableStyleElement1 = tableStyleElements.Add(ExcelTableStyleElementType.FirstColumn);
            tableStyleElement1.FontColorRGB = System.Drawing.Color.FromArgb(128, 128, 128);

            ITableStyleElement tableStyleElement2 = tableStyleElements.Add(ExcelTableStyleElementType.HeaderRow);
            tableStyleElement2.FontColor = ExcelKnownColors.White;
            tableStyleElement2.BackColorRGB = System.Drawing.Color.FromArgb(0, 112, 192);
        }

        return table;
    }
    public override void CreateWorksheet<T>(DataTable dataTable)
    {
        _worksheet = _workbook.Worksheets.Create(dataTable.TableName);
        _worksheet.ImportDataTable(dataTable, true, 1, 1);
    }
    public override void CreateWorksheet<T>(string name, List<T> data)
    {
        _worksheet = _workbook.Worksheets.Create(name);
        var dataTable = CreateDataTable<T>(data);
        _worksheet.ImportDataTable(dataTable, true, 1, 1);
    }
    public override DataTable GetDataFromFile(string filePath)
    {
        _workbook = _application.Workbooks.Open(filePath);
        _worksheet = _workbook.Worksheets.FirstOrDefault();
        if (_worksheet is not null)
        {
            var dataTable = _worksheet.ExportDataTable(_worksheet.UsedRange, ExcelExportDataTableOptions.ColumnNames);
            return dataTable;
        }
        return new DataTable();
    }
    public override void Save(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            return;

        using (FileStream outputStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
        {
            _workbook.SaveAs(outputStream);
        }
    }
    public override void Dispose()
    {
        _excelEngine.Dispose();
    }

}
