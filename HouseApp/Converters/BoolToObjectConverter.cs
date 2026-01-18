using System.Globalization;

namespace HouseApp.Converters;

public class BoolToObjectConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not bool boolValue || parameter is not string parameterString)
            return parameter;

        // Split the parameter string by '|' delimiter
        // Format: "TrueValue|FalseValue"
        var values = parameterString.Split('|');
        
        if (values.Length != 2)
            return parameter;

        return boolValue ? values[0] : values[1];
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}