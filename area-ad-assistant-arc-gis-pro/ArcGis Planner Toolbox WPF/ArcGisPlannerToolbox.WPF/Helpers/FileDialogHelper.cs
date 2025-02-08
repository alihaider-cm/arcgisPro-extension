using System.Windows.Forms;

namespace ArcGisPlannerToolbox.WPF.Helpers;

public class FileDialogHelper
{
    public static string GetFilePathFromFolderDialog()
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
