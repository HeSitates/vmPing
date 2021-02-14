using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;

namespace vmPing.Classes
{
  public class CommandLine
  {
    public static List<string> ParseArguments()
    {
      var args = Environment.GetCommandLineArgs();
      var errorMessage = string.Empty;
      var hostnames = new List<string>();

      const int minimumInterval = 1;
      const int maxInterval = 86400;
      const int minimumTimeout = 1;
      const int maxTimeout = 60;
      var helpText = $"Command Line Usage:{Environment.NewLine}vmPing [-i interval] [-w timeout] [<target_host>...] [<path_to_list_of_hosts>...]";

      for (var index = 1; index < args.Length; ++index)
      {
        var param = args[index];
        switch (param.ToLower())
        {
          case "/f":
          case "-f":
            index = ReadFavorite(index, args, ref errorMessage);
            break;
          case "/?":
          case "-?":
          case "-h":
          case "--help":
            MessageBox.Show(helpText, "vmPing Help", MessageBoxButton.OK, MessageBoxImage.Information);
            Application.Current.Shutdown();
            break;
          case "/i":
          case "-i":
            index = ReadInterval(index, args, minimumInterval, maxInterval, ref errorMessage);
            break;
          case "/m":
          case "-m":
            ApplicationOptions.WindowState = WindowState.Maximized;
            break;
          case "/w":
          case "-w":
            index = ReadPingTimeout(args, index, minimumTimeout, maxTimeout, ref errorMessage);
            break;
          default:
            // If an invalid argument is supplied, check to see if the argument is a valid path name.
            //   If so, attempt to parse and read hosts from the file.  If not, use the argument as a hostname.
            if (File.Exists(param))
            {
              hostnames.AddRange(ReadHostsFromFile(param));
            }
            else
            {
              hostnames.Add(param);
            }

            break;
        }
      }

      // Display error message if any problems were encountered while parsing the arguments.
      if (errorMessage.Length > 0)
      {
        MessageBox.Show($"{errorMessage}{Environment.NewLine}{Environment.NewLine}{helpText}", "vmPing Error", MessageBoxButton.OK, MessageBoxImage.Error);
        Application.Current.Shutdown();
      }

      for (var i = 0; i < hostnames.Count; ++i)
      {
        hostnames[i] = hostnames[i].ToUpper();
      }

      return hostnames;
    }

    private static int ReadFavorite(int index, string[] args, ref string errorMessage)
    {
      var favoriteTitle = string.Empty;
      if (index + 1 < args.Length)
      {
        favoriteTitle = args[index + 1];
      }

      if (!string.IsNullOrWhiteSpace(favoriteTitle) && !favoriteTitle.StartsWith("-"))
      {
        ApplicationOptions.FavoriteToStartWith = favoriteTitle;

        ++index;
      }
      else
      {
        errorMessage += $"For switch -f you must specify a favorite to load.{Environment.NewLine}";
        return index;
      }

      return index;
    }

    private static int ReadInterval(int index, string[] args, int minimumInterval, int maxInterval, ref string errorMessage)
    {
      if (index + 1 < args.Length && int.TryParse(args[index + 1], out var interval) && interval >= minimumInterval && interval <= maxInterval)
      {
        ApplicationOptions.PingInterval = interval * 1000;
        ++index;
      }
      else
      {
        errorMessage += $"For switch -i you must specify the number of seconds between {minimumInterval} and {maxInterval}.{Environment.NewLine}";
        return index;
      }

      return index;
    }

    private static int ReadPingTimeout(string[] args, int index, int minimumTimeout, int maxTimeout, ref string errorMessage)
    {
      if (args.Length > index + 1 && int.TryParse(args[index + 1], out var timeout) && timeout >= minimumTimeout && timeout <= maxTimeout)
      {
        ApplicationOptions.PingTimeout = timeout * 1000;
        ++index;
      }
      else
      {
        errorMessage += $"For switch -w you must specify the number of seconds between {minimumTimeout} and {maxTimeout}.{Environment.NewLine}";
        return index;
      }

      return index;
    }

    private static IEnumerable<string> ReadHostsFromFile(string path)
    {
      try
      {
        var linesInFile = new List<string>(File.ReadAllLines(path));
        var hostsInFile = new List<string>();

        foreach (var line in linesInFile)
        {
          if (line == string.Empty)
          {
            continue;
          }

          if (!char.IsLetterOrDigit(line[0]))
          {
            continue;
          }

          hostsInFile.Add(line.Trim());
        }

        return hostsInFile;
      }
      catch
      {
        MessageBox.Show($"Failed parsing {path}", "vmPing Error", MessageBoxButton.OK, MessageBoxImage.Error);
        return new List<string>();
      }
    }
  }
}
