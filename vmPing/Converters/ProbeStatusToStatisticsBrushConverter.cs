namespace vmPing.Converters
{
  public class ProbeStatusToStatisticsBrushConverter : System.Windows.Data.IValueConverter
  {
    public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      if(value == null)
      {
        return (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFromString(vmPing.Classes.ApplicationOptions.ForegroundColor_Stats_Inactive);
      }

      switch ((vmPing.Classes.ProbeStatus)value)
      {
        case vmPing.Classes.ProbeStatus.Up:
          return (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFromString(vmPing.Classes.ApplicationOptions.ForegroundColor_Stats_Up);
        case vmPing.Classes.ProbeStatus.Down:
          return (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFromString(vmPing.Classes.ApplicationOptions.ForegroundColor_Stats_Down);
        case vmPing.Classes.ProbeStatus.Error:
          return (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFromString(vmPing.Classes.ApplicationOptions.ForegroundColor_Stats_Error);
        case vmPing.Classes.ProbeStatus.Indeterminate:
          return (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFromString(vmPing.Classes.ApplicationOptions.ForegroundColor_Stats_Indeterminate);
        default:
          return (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFromString(vmPing.Classes.ApplicationOptions.ForegroundColor_Stats_Inactive);
      }
    }

    public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      throw new System.NotImplementedException();
    }
  }
}