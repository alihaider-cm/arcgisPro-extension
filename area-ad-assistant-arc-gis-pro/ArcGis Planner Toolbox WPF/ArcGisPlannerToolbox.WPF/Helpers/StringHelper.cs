using System.Text.RegularExpressions;

namespace ArcGisPlannerToolbox.WPF.Helpers;

public class StringHelper
{
    public static string RemoveSpecialCharacters(string text) => Regex.Replace(text, "[^0-9A-Za-z _-]", string.Empty);
    public static string GetOnlyNumericValue(string text) => Regex.Replace(text, @"[^\d]", string.Empty);
}
