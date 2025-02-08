using System.Collections.Generic;
using System.Text;

namespace ArcGisPlannerToolbox.WPF.Extensions;

public static class stringExtension
{
    public static string CreateQuery<T>(this List<T> values, string identifier, bool withStringSeparator = true)
    {
        var builder = new StringBuilder();
        if (withStringSeparator)
        {
            builder.AppendJoin("', '", values);
            builder.Insert(0, "'");
        }
        else
            builder.AppendJoin(",", values);
        //builder.Append("'");
        builder.Insert(0, $"{identifier} in (");
        if (withStringSeparator)
            builder.Append("')");
        else
            builder.Append(")");
        return builder.ToString();
    }
}
