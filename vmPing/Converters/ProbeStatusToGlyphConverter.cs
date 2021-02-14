namespace vmPing.Converters
{
  public class ProbeStatusToGlyphConverter : System.Windows.Data.IValueConverter
  {
    public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      if (value == null)
      {
        return string.Empty;
      }

      var returnString = string.Empty;
      switch ((vmPing.Classes.ProbeStatus)value)
      {
        case vmPing.Classes.ProbeStatus.Error:
          //returnString = "r";
          break;
        case vmPing.Classes.ProbeStatus.Down:
          returnString = "u";
          break;
        case vmPing.Classes.ProbeStatus.Indeterminate:
          returnString = "i";
          break;
        case vmPing.Classes.ProbeStatus.Up:
          returnString = "t";
          break;
      }

      return returnString;
    }

    public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      throw new System.NotImplementedException();
    }
  }
}