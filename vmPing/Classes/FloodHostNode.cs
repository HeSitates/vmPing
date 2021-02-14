using System;
using System.ComponentModel;
using System.Threading;

namespace vmPing.Classes
{
  public class FloodHostNode : INotifyPropertyChanged
  {
    private bool isActive;
    private long packetsSent;
    private long packetsReceived;
    private long packetsLost;

    public event PropertyChangedEventHandler PropertyChanged;

    public DateTime StartTime { get; set; }

    public bool IsActive
    {
      get => isActive;
      set
      {
        if (value != isActive)
        {
          isActive = value;
          NotifyPropertyChanged(nameof(IsActive));
        }
      }
    }

    public long PacketsSent
    {
      get => packetsSent;
      set
      {
        if (value != packetsSent)
        {
          packetsSent = value;
          NotifyPropertyChanged(nameof(PacketsSent));
        }
      }
    }

    public long PacketsReceived
    {
      get => packetsReceived;
      set
      {
        if (value != packetsReceived)
        {
          packetsReceived = value;
          NotifyPropertyChanged(nameof(PacketsReceived));
        }
      }
    }

    public long PacketsLost
    {
      get => packetsLost;
      set
      {
        if (value != packetsLost)
        {
          packetsLost = value;
          NotifyPropertyChanged(nameof(PacketsLost));
        }
      }
    }

    public string DestinationAddress { get; set; }

    public BackgroundWorker BgWorker { get; set; }

    public AutoResetEvent ResetEvent { get; set; }


    private void NotifyPropertyChanged(string info)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
    }
  }
}
