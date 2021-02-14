namespace vmPing.Converters
{
  public class ButtonTextVisibilityConverter : System.Windows.Data.IValueConverter
  {
    public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      if (value != null && (double)value > 300)
      {
        return System.Windows.Visibility.Visible;
      }
      else
      {
        return System.Windows.Visibility.Collapsed;
      }
    }

    public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      throw new System.NotImplementedException();
    }
  }
}