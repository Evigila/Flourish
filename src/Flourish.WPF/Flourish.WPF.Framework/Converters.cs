using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ArkheideSystem.Flourish.WPF;

public sealed class TextVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) => string.IsNullOrEmpty(value?.ToString()) ? Visibility.Collapsed : Visibility.Visible;
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
}
public sealed class PercentageConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) => values.Length >= 3 && values[0] is double value && values[1] is double min && values[2] is double max && max > min ? $"{(value - min) / (max - min) * 100:0}%" : "0%";
    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => targetTypes.Select(_ => Binding.DoNothing).ToArray();
}
public sealed class StackWordsConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) => values[0] is string title ? values.Length > 1 && values[1] is true ? title.Replace(' ', '\n') : title : "";
    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => targetTypes.Select(_ => Binding.DoNothing).ToArray();
}
