using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;

namespace TableManager.UI.Converters;

public class FirstErrorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is List<string> errors && errors.Count > 0)
        {
            return errors[0];
        }
        return string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
