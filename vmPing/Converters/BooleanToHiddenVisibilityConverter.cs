namespace vmPing.Converters
{
  public class BooleanToHiddenVisibilityConverter : System.Windows.Data.IValueConverter
  {
    public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      if (value != null && (bool)value == false)
      {
        return System.Windows.Visibility.Hidden;
      }
      else
      {
        return System.Windows.Visibility.Visible;
      }
    }

    public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      throw new System.NotImplementedException();
    }
  }
}