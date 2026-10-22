using System.Globalization;

namespace StudentCabinetMVVM.Converters;

public class HighScoreColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is double score && score >= 4.0)
            return Colors.Green;

        return Colors.Red;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}