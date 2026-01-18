using System.Globalization;

namespace HouseApp.Converters;

public class BoolToColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isConnected && isConnected)
        {
            // Green when connected
            return Color.FromArgb("#10B981");
        }
        
        // Red when disconnected
        return Color.FromArgb("#EF4444");
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}