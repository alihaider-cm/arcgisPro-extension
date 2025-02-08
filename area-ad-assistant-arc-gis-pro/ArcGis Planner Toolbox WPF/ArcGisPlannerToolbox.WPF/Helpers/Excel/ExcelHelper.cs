using System;
using System.Collections.Generic;
using System.Data;

namespace ArcGisPlannerToolbox.WPF.Helpers.Excel;

public abstract class ExcelHelper : IDisposable
{
    public abstract DataTable GetDataFromFile(string filePath);
    public abstract DataTable CreateDataTable<T>(List<T> data) where T : class;
    public abstract void CreateWorksheet<T>(string name, List<T> data) where T : class;
    public abstract void CreateWorksheet<T>(DataTable dataTable) where T : class;
    public abstract void Dispose();
    public abstract void Save(string filePath);
}
