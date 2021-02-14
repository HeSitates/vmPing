namespace vmPing.Converters
{
  public class StringToBrushConverter : System.Windows.Data.IValueConverter
  {
    public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      if (string.IsNullOrWhiteSpace((string)value))
      {
        return System.Windows.Data.Binding.DoNothing;
      }

      try
      {
        return (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFromString((string)value);
      }
      catch
      {
        return System.Windows.Data.Binding.DoNothing;
      }
    }

    public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      throw new System.NotImplementedException();
    }
  }
}