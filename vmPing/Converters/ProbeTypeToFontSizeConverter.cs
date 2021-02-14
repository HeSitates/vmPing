namespace vmPing.Converters
{
  public class ProbeTypeToFontSizeConverter : System.Windows.Data.IValueConverter
  {
    public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      if (value == null)
      {
        return vmPing.Classes.ApplicationOptions.FontSize_Scanner;
      }

      switch ((vmPing.Classes.ProbeType)value)
      {
        case vmPing.Classes.ProbeType.Ping:
          return vmPing.Classes.ApplicationOptions.FontSize_Probe;
        default:
          return vmPing.Classes.ApplicationOptions.FontSize_Scanner;
      }
    }

    public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      throw new System.NotImplementedException();
    }
  }
}