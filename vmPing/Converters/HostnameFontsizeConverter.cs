namespace vmPing.Converters
{
  public class HostnameFontsizeConverter : System.Windows.Data.IValueConverter
  {
    public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      if (value == null)
      {
        return 12.5;
      }

      var doubleValue = (double)value;
      if (doubleValue > 250)
      {
        return 18;
      }
      else if (doubleValue > 225)
      {
        return 17;
      }
      else if (doubleValue > 200)
      {
        return 16;
      }
      else if (doubleValue > 175)
      {
        return 15;
      }
      else if (doubleValue > 150)
      {
        return 14;
      }
      else if (doubleValue > 125)
      {
        return 13;
      }
      else
      {
        return 12.5;
      }
    }

    public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      throw new System.NotImplementedException();
    }
  }
}