using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using vmPing.Properties;
using vmPing.Views;

namespace vmPing.Classes
{
  public partial class Probe : INotifyPropertyChanged
  {
    public static ObservableCollection<StatusChangeLog> StatusChangeLog = new ObservableCollection<StatusChangeLog>();
    public static StatusHistoryWindow StatusWindow;

    private static readonly Mutex _mutex = new Mutex();
    private static          int   _activeCount;

    private ObservableCollection<string> _history;
    private ProbeType                    _type = ProbeType.Ping;
    private string                       _hostname;
    private string                       _alias;
    private ProbeStatus                  _status = ProbeStatus.Inactive;
    private bool                         _isActive;
    private string                       _statisticsText;

    public static int ActiveCount
    {
      get => _activeCount;
      set
      {
        _activeCount = value;
        OnActiveCountChanged(EventArgs.Empty);
      }
    }
    
    public static event EventHandler ActiveCountChanged;
    
    protected static void OnActiveCountChanged(EventArgs e)
    {
      ActiveCountChanged?.Invoke(null, e);
    }

    public event PropertyChangedEventHandler PropertyChanged;

    public IsolatedPingWindow IsolatedWindow { get; set; }
    
    public int IndeterminateCount { get; set; }
    
    public PingStatistics Statistics { get; set; }
    
    public CancellationTokenSource CancelSource { get; set; }
    
    public ObservableCollection<string> History
    {
      get => _history;
      set
      {
        _history = value;
        NotifyPropertyChanged(nameof(History));
      }
    }

    public ProbeType Type
    {
      get => _type;
      set
      {
        _type = value;
        NotifyPropertyChanged(nameof(Type));
      }
    }

    public string Hostname
    {
      get => _hostname;
      set
      {
        if (value != _hostname)
        {
          _hostname = value.Trim();
          NotifyPropertyChanged(nameof(Hostname));
        }
      }
    }

    public string Alias
    {
      get => _alias;
      set
      {
        _alias = value;
        NotifyPropertyChanged(nameof(Alias));
      }
    }

    public ProbeStatus Status
    {
      get => _status;
      set
      {
        _status = value;
        NotifyPropertyChanged(nameof(Status));
      }
    }

    public bool IsActive
    {
      get => _isActive;
      set
      {
        if (value == _isActive)
        {
          return;
        }

        _isActive = value;
        NotifyPropertyChanged(nameof(IsActive));

        _mutex.WaitOne();
        if (value)
        {
          ++ActiveCount;
        }
        else
        {
          --ActiveCount;
        }

        _mutex.ReleaseMutex();
        NotifyPropertyChanged("NumberOfActivePings");
      }
    }

    public string StatisticsText
    {
      get => _statisticsText;
      set
      {
        if (value != _statisticsText)
        {
          _statisticsText = value;
          NotifyPropertyChanged(nameof(StatisticsText));
        }
      }
    }
    
    public void AddHistory(string historyItem)
    {
      const int maxSize = 3600;

      History.Add(historyItem);
      if (History.Count > maxSize)
      {
        History.RemoveAt(0);
      }
    }

    public void WriteFinalStatisticsToHistory()
    {
      if (Statistics == null || Statistics.Sent == 0)
      {
        return;
      }

      var roundTripTimes = new List<int>();
      var rttRegex = new Regex($@"  \[(?<rtt><?\d+) ?{Strings.Milliseconds_Symbol}]$");

      foreach (var historyItem in History)
      {
        var regexMatch = rttRegex.Match(historyItem);
        if (!regexMatch.Success)
        {
          continue;
        }

        roundTripTimes.Add(regexMatch.Groups["rtt"].Value == "<1" ? 0 : int.Parse(regexMatch.Groups["rtt"].Value));
      }

      // Display stats and round trip times.
      AddHistory("");
      AddHistory(
          $"Sent {Statistics.Sent}, " +
          $"Received {Statistics.Received}, " +
          $"Lost {Statistics.Sent - Statistics.Received} ({(100 * (Statistics.Sent - Statistics.Received)) / Statistics.Sent}% loss)");
      if (roundTripTimes.Count > 0)
      {
        AddHistory(
                   $"Minimum ({roundTripTimes.Min()}{Strings.Milliseconds_Symbol}), " +
                   $"Maximum ({roundTripTimes.Max()}{Strings.Milliseconds_Symbol}), " +
                   $"Average ({roundTripTimes.Average():0.##}{Strings.Milliseconds_Symbol})");
      }

      AddHistory(" ");
    }

    private void NotifyPropertyChanged(string info)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
    }
  }
}
