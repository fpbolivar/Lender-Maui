using System.Globalization;

namespace Lender.Converters;

/// <summary>
/// Converts a boolean value to its inverted (negated) value.
/// </summary>
public class InvertedBoolConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return !boolValue;
        }
        return value ?? false;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return !boolValue;
        }
        return value ?? false;
    }
}

/// <summary>
/// Converts a string value to a boolean indicating if it is null or empty.
/// </summary>
public class StringNullOrEmptyConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string str)
        {
            // parameter is true when we want to invert (show when NOT null/empty)
            bool isNullOrEmpty = string.IsNullOrWhiteSpace(str);
            
            if (parameter is string paramStr && paramStr == "True")
            {
                return !isNullOrEmpty;  // Return true if NOT null/empty (for Visibility binding)
            }
            
            return isNullOrEmpty;
        }
        return true;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converts a boolean value to a color (true = AccentOrange, false = CardBg).
/// </summary>
public class BoolToColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            // When true, return AccentOrange color; when false, return CardBg color
            return boolValue 
                ? Color.FromArgb("#FF9F43")  // AccentOrange
                : Color.FromArgb("#132F4C"); // CardBg
        }
        return Color.FromArgb("#132F4C");
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converts a boolean value to opacity (true = 1.0, false = 0.5).
/// </summary>
public class BoolToOpacityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return boolValue ? 1.0 : 0.5;
        }
        return 1.0;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
