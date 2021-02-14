using System.ComponentModel;
using System.Net.NetworkInformation;

namespace vmPing.Classes
{
  public class NetworkRouteNode : INotifyPropertyChanged
  {
    public event PropertyChangedEventHandler PropertyChanged;
    
    private string _hostAddress;
    private string _hostName;
    private long   _roundTripTime;

    public string HostAddress
    {
      get => _hostAddress;
      set
      {
        if (value != _hostAddress)
        {
          _hostAddress = value;
          NotifyPropertyChanged(nameof(HostAddress));
        }
      }
    }

    public string HostName
    {
      get => _hostName;
      set
      {
        if (value != _hostName)
        {
          _hostName = value;
          NotifyPropertyChanged(nameof(HostName));
        }
      }
    }

    public long RoundTripTime
    {
      get => _roundTripTime;
      set
      {
        if (value != _roundTripTime)
        {
          _roundTripTime = value;
          NotifyPropertyChanged(nameof(RoundTripTime));
        }
      }
    }

    public IPStatus ReplyStatus { get; set; }

    public int HopId { get; set; }

    private void NotifyPropertyChanged(string info)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
    }
  }
}
