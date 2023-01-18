using System;
using System.Globalization;
using Avalonia.Data.Converters;
using NexNux.Utilities;

namespace NexNux.Converters;

public class IsModItemFolderConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is ModFolderItem;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}