namespace vmPing.Converters
{
  public class ProbeCountToGlobalStartStopIcon : System.Windows.Data.IValueConverter
  {
    public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      if (value != null && (int)value > 0)
      {
        return new System.Windows.Media.Imaging.BitmapImage(new System.Uri(@"/Resources/stopCircle-16.png", System.UriKind.Relative));
      }
      else
      {
        return new System.Windows.Media.Imaging.BitmapImage(new System.Uri(@"/Resources/play-16.png", System.UriKind.Relative));
      }
    }

    public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      throw new System.NotImplementedException();
    }
  }
}