using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace vmPing.Converters
{
  public class ProbeTypeToVisibilityConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (value == null)
      {
        return Visibility.Collapsed;
      }

      switch ((vmPing.Classes.ProbeType)value)
      {
        case vmPing.Classes.ProbeType.Ping:
          return Visibility.Visible;
        default:
          return Visibility.Collapsed;
      }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }
}
