using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace DinnersReady.Helpers;

public class PrefixConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string text && !string.IsNullOrWhiteSpace(text))
        {
            int index = text.IndexOf(' ');
            return index > 0 ? text.Substring(0, index) : string.Empty;
        }
        return string.Empty;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotImplementedException();
}

public class BodyConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string text && !string.IsNullOrWhiteSpace(text))
        {
            int index = text.IndexOf(' ');
            return index > 0 ? text.Substring(index + 1) : text;
        }
        return value;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotImplementedException();
}
