﻿using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using vmPing.Classes;

namespace vmPing.Views
{
  /// <summary>
  /// TraceRouteWindow is a tool for performing a network traceroute to a given host.
  /// </summary>
  public partial class TraceRouteWindow
  {
    private NetworkRoute _route = new NetworkRoute();

    internal NetworkRoute Route { get => _route; set => _route = value; }

    public TraceRouteWindow()
    {
      InitializeComponent();

      DataContext = _route;
      dgTrace.ItemsSource = _route.networkRoute;

      // Set initial focus to text box.
      Loaded += (sender, e) =>
          MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
    }

    private void btnTraceRoute_Click(object sender, RoutedEventArgs e)
    {
      if (!_route.IsActive)
      {
        if (txtHostname.Text.Length == 0)
        {
          return;
        }

        _route.BgWorker?.CancelAsync();

        tbTraceStatus.Text = "Tracing Route...";
        _route.DestinationHost = txtHostname.Text;
        _route.MaxHops = 30;
        _route.PingTimeout = 2000;
        _route.networkRoute.Clear();
        _route.IsActive = true;

        _route.BgWorker = new BackgroundWorker();
        _route.ResetEvent = new AutoResetEvent(false);
        _route.BgWorker.DoWork += backgroundThread_TraceRoute;
        _route.BgWorker.ProgressChanged += backgroundThread_ProgressChanged;
        _route.BgWorker.WorkerSupportsCancellation = true;
        _route.BgWorker.WorkerReportsProgress = true;
        _route.BgWorker.RunWorkerAsync();
      }
      else
      {
        _route.BgWorker.CancelAsync();
        _route.ResetEvent.WaitOne();
        _route.IsActive = false;
        tbTraceStatus.Text = "Trace Cancelled";
      }
    }

    public void backgroundThread_TraceRoute(object sender, DoWorkEventArgs e)
    {
      if (!(sender is BackgroundWorker bgWorker))
      {
        return;
      }

      var pingBuffer  = Encoding.ASCII.GetBytes(Constants.DefaultIcmpData);
      var pingOptions = new PingOptions(1, true);
      var timer       = new Stopwatch();
      _route.Timer = timer;

      while (!bgWorker.CancellationPending && _route.IsActive && pingOptions.Ttl <= _route.MaxHops)
      {
        if (IPAddress.TryParse(_route.DestinationHost, out var ipAddress))
        {
          _route.DestinationIp = ipAddress;
        }
        else
        {
          try { _route.DestinationIp = Dns.GetHostEntry(_route.DestinationHost).AddressList[0]; }
          catch { bgWorker.ReportProgress(-1); break; }
        }

        using (var ping = new Ping())
        {
          try
          {
            _route.Timer.Reset();
            _route.Timer.Start();
            var pingReply = ping.Send(_route.DestinationIp, _route.PingTimeout, pingBuffer, pingOptions);
            if (pingReply != null && pingReply.Status == IPStatus.TimedOut)
            {
              _route.Timer.Reset();
              _route.Timer.Start();
              pingReply = ping.Send(_route.DestinationIp, _route.PingTimeout, pingBuffer, pingOptions);
            }

            if (!bgWorker.CancellationPending)
            {
              bgWorker.ReportProgress(pingOptions.Ttl, pingReply);
            }

            if (pingReply != null && pingReply.Status == IPStatus.Success)
            {
              break;
            }

            _route.ResetEvent.Set();
            Thread.Sleep(100);
            ++pingOptions.Ttl;
          }

          catch { break; }
        }
      }

      e.Cancel = true;
      _route.ResetEvent.Set();
      _route.IsActive = false;
    }

    private void backgroundThread_ProgressChanged(object sender, ProgressChangedEventArgs e)
    {
      _route.Timer.Stop();

      if (e.ProgressPercentage < 0)
      {
        tbTraceStatus.Text = "Invalid Hostname";
        return;
      }

      var pingReply = e.UserState as PingReply;
      var node = new NetworkRouteNode();

      if (pingReply?.Address != null)
      {
        node.HostAddress = pingReply.Address.ToString();
      }

      node.ReplyStatus   = pingReply?.Status ?? IPStatus.Unknown;
      node.HopId         = e.ProgressPercentage;
      node.RoundTripTime = _route.Timer.ElapsedMilliseconds;

      switch (node.ReplyStatus)
      {
        case IPStatus.TimedOut:
          node.HostAddress = "Timed Out";
          break;
        case IPStatus.Success:
          tbTraceStatus.Text = "Trace Complete";
          break;
      }

      _route.networkRoute.Add(node);
      dgTrace.ScrollIntoView(dgTrace.Items[_route.networkRoute.Count - 1]);
    }
  }
}
