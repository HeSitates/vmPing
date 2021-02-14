namespace vmPing.Converters
{
  public class BoolToValueConverter<T> : System.Windows.Data.IValueConverter
  {
    public T FalseValue { get; set; }
        
    public T TrueValue { get; set; }

    public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      if (value == null)
      {
        return FalseValue;
      }
      else
      {
        return (bool)value ? TrueValue : FalseValue;
      }
    }

    public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      return value != null && value.Equals(TrueValue);
    }
  }
}