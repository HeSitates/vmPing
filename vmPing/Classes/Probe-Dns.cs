using System;
using System.Collections.ObjectModel;
using System.Net;
using System.Threading;

namespace vmPing.Classes
{
  public partial class Probe
  {
    private async void PerformDnsLookup(CancellationToken cancellationToken)
    {
      IsActive = true;
      History = new ObservableCollection<string>();
      Status = ProbeStatus.Scanner;

      try
      {
        AddHistory($"[\u2022] Resolving {Hostname}:{Environment.NewLine}");
        
        switch (Uri.CheckHostName(Hostname))
        {
          case UriHostNameType.IPv4:
          case UriHostNameType.IPv6:
            await PerformDnsLookupForIpAdddress(cancellationToken);
            break;
          case UriHostNameType.Dns:
            await PerformDnsLookupForHostname(cancellationToken);
            break;
          default:
            throw new Exception();
        }

        AddHistory($"{Environment.NewLine}{Environment.NewLine}\u2605 Done");
      }
      catch
      {
        if (!cancellationToken.IsCancellationRequested)
        {
          AddHistory($"{Environment.NewLine}\u2605 Unable to resolve hostname");
        }
      }
      finally
      {
        IsActive = false;
      }
    }

    private async System.Threading.Tasks.Task PerformDnsLookupForHostname(CancellationToken cancellationToken)
    {
      var ipAddresses = await Dns.GetHostAddressesAsync(Hostname);
      cancellationToken.ThrowIfCancellationRequested();
      foreach (var ip in ipAddresses)
      {
        AddHistory($"    {ip}");
      }
    }

    private async System.Threading.Tasks.Task PerformDnsLookupForIpAdddress(CancellationToken cancellationToken)
    {
      var host = await Dns.GetHostEntryAsync(Hostname);
      cancellationToken.ThrowIfCancellationRequested();
      if (host != null)
      {
        AddHistory($"    {host.HostName}");
      }
    }
  }
}