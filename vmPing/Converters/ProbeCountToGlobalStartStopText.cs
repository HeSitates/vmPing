namespace vmPing.Converters
{
  public class ProbeCountToGlobalStartStopText : System.Windows.Data.IValueConverter
  {
    public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      if (value != null && (int)value > 0)
      {
        return vmPing.Properties.Strings.Toolbar_StopAll;
      }
      else
      {
        return vmPing.Properties.Strings.Toolbar_StartAll;
      }
    }

    public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      throw new System.NotImplementedException();
    }
  }
}