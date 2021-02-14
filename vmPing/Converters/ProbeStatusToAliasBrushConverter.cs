namespace vmPing.Converters
{
  public class ProbeStatusToAliasBrushConverter : System.Windows.Data.IValueConverter
  {
    public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      if (value == null)
      {
        return (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFromString(vmPing.Classes.ApplicationOptions.ForegroundColor_Alias_Inactive);
      }

      switch ((vmPing.Classes.ProbeStatus)value)
      {
        case vmPing.Classes.ProbeStatus.Up:
          return (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFromString(vmPing.Classes.ApplicationOptions.ForegroundColor_Alias_Up);
        case vmPing.Classes.ProbeStatus.Down:
          return (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFromString(vmPing.Classes.ApplicationOptions.ForegroundColor_Alias_Down);
        case vmPing.Classes.ProbeStatus.Error:
          return (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFromString(vmPing.Classes.ApplicationOptions.ForegroundColor_Alias_Error);
        case vmPing.Classes.ProbeStatus.Indeterminate:
          return (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFromString(vmPing.Classes.ApplicationOptions.ForegroundColor_Alias_Indeterminate);
        case vmPing.Classes.ProbeStatus.Scanner:
          return (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFromString(vmPing.Classes.ApplicationOptions.ForegroundColor_Alias_Scanner);
        default:
          return (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFromString(vmPing.Classes.ApplicationOptions.ForegroundColor_Alias_Inactive);
      }
    }

    public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      throw new System.NotImplementedException();
    }
  }
}