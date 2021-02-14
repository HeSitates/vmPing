using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using vmPing.Classes;
using vmPing.Properties;

namespace vmPing.Views
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow
  {
    private readonly ObservableCollection<Probe> _probeCollection = new ObservableCollection<Probe>();
    private          Dictionary<string, string>  _aliases         = new Dictionary<string, string>();

    public static RoutedCommand ProbeOptionsCommand = new RoutedCommand();
    public static RoutedCommand StartStopCommand = new RoutedCommand();
    public static RoutedCommand HelpCommand = new RoutedCommand();
    public static RoutedCommand NewInstanceCommand = new RoutedCommand();
    public static RoutedCommand TraceRouteCommand = new RoutedCommand();
    public static RoutedCommand FloodHostCommand = new RoutedCommand();
    public static RoutedCommand AddMonitorCommand = new RoutedCommand();


    public MainWindow()
    {
      InitializeComponent();
      InitializeApplication();
    }


    private void InitializeApplication()
    {
      InitializeCommandBindings();
      Configuration.UpgradeConfigurationFile();
      LoadFavorites();
      LoadAliases();
      Configuration.Load();
      UpdatePopupOptionIsCheckedState();

      var hosts = CommandLine.ParseArguments();
      if (!string.IsNullOrWhiteSpace(ApplicationOptions.FavoriteToStartWith))
      {
        StartFavorite(ApplicationOptions.FavoriteToStartWith);
      }
      else
      {
        if (hosts.Count > 0)
        {
          AddProbe(hosts.Count);
          for (var i = 0; i < hosts.Count; ++i)
          {
            _probeCollection[i].Hostname = hosts[i].ToUpper();
            _probeCollection[i].Alias = _aliases.ContainsKey(_probeCollection[i].Hostname) ? _aliases[_probeCollection[i].Hostname] : null;
            _probeCollection[i].StartStop();
          }
        }
        else
        {
          AddProbe(2);
        }

        ColumnCount.Value = _probeCollection.Count;
      }

      ProbeItemsControl.ItemsSource = _probeCollection;
      if (ApplicationOptions.WindowState == WindowState.Maximized)
      {
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
        SourceInitialized += (s, a) => WindowState = WindowState.Maximized;
      }
    }


    private void UpdatePopupOptionIsCheckedState()
    {
      PopupAlways.IsChecked = false;
      PopupNever.IsChecked = false;
      PopupWhenMinimized.IsChecked = false;

      switch (ApplicationOptions.PopupOption)
      {
        case ApplicationOptions.PopupNotificationOption.Always:
          PopupAlways.IsChecked = true;
          break;
        case ApplicationOptions.PopupNotificationOption.Never:
          PopupNever.IsChecked = true;
          break;
        case ApplicationOptions.PopupNotificationOption.WhenMinimized:
          PopupWhenMinimized.IsChecked = true;
          break;
      }
    }


    private void InitializeCommandBindings()
    {
      CommandBindings.Add(new CommandBinding(ProbeOptionsCommand, ProbeOptionsExecute));
      CommandBindings.Add(new CommandBinding(StartStopCommand, StartStopExecute));
      CommandBindings.Add(new CommandBinding(HelpCommand, HelpExecute));
      CommandBindings.Add(new CommandBinding(NewInstanceCommand, NewInstanceExecute));
      CommandBindings.Add(new CommandBinding(TraceRouteCommand, TraceRouteExecute));
      CommandBindings.Add(new CommandBinding(FloodHostCommand, FloodHostExecute));
      CommandBindings.Add(new CommandBinding(AddMonitorCommand, AddMonitorExecute));

      var kgProbeOptions = new KeyGesture(Key.F10);
      var kgStartStop = new KeyGesture(Key.F5);
      var kgHelp = new KeyGesture(Key.F1);
      var kgNewInstance = new KeyGesture(Key.N, ModifierKeys.Control);
      var kgTraceRoute = new KeyGesture(Key.T, ModifierKeys.Control);
      var kgFloodHost = new KeyGesture(Key.F, ModifierKeys.Control);
      var kgAddMonitor = new KeyGesture(Key.A, ModifierKeys.Control);
      InputBindings.Add(new InputBinding(ProbeOptionsCommand, kgProbeOptions));
      InputBindings.Add(new InputBinding(StartStopCommand, kgStartStop));
      InputBindings.Add(new InputBinding(HelpCommand, kgHelp));
      InputBindings.Add(new InputBinding(NewInstanceCommand, kgNewInstance));
      InputBindings.Add(new InputBinding(TraceRouteCommand, kgTraceRoute));
      InputBindings.Add(new InputBinding(FloodHostCommand, kgFloodHost));
      InputBindings.Add(new InputBinding(AddMonitorCommand, kgAddMonitor));

      StartStopMenu.Command = StartStopCommand;
      HelpMenu.Command = HelpCommand;
      NewInstanceMenu.Command = NewInstanceCommand;
      TraceRouteMenu.Command = TraceRouteCommand;
      FloodHostMenu.Command = FloodHostCommand;
      AddMonitorMenu.Command = AddMonitorCommand;
    }


    public void AddProbe(int numberOfProbes = 1)
    {
      for (; numberOfProbes > 0; --numberOfProbes)
      {
        _probeCollection.Add(new Probe());
      }
    }


    public void ProbeStartStop_Click(object sender, EventArgs e)
    {
      ((Probe)((Button)sender).DataContext).StartStop();
    }


    private void ColumnCount_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
      if (ColumnCount.Value > _probeCollection.Count)
      {
        ColumnCount.Value = _probeCollection.Count;
      }
    }


    private void Hostname_KeyDown(object sender, KeyEventArgs e)
    {
      if (e.Key == Key.Enter)
      {
        var probe = (sender as TextBox)?.DataContext as Probe;
        if (probe == null)
        {
          return;
        }

        probe.StartStop();

        if (_probeCollection.IndexOf(probe) >= _probeCollection.Count - 1)
        {
          return;
        }

        var cp = ProbeItemsControl.ItemContainerGenerator.ContainerFromIndex(_probeCollection.IndexOf(probe) + 1) as ContentPresenter;
        var tb = (TextBox)cp?.ContentTemplate.FindName("Hostname", cp);

        tb?.Focus();
      }
    }


    private void RemoveProbe_Click(object sender, RoutedEventArgs e)
    {
      if (_probeCollection.Count <= 1)
      {
        return;
      }

      var probe = (Probe)((Button)sender).DataContext;
      if (probe.IsActive)
      {
        // Stop/cancel active probe.
        probe.StartStop();
      }

      _probeCollection.Remove(probe);
      if (ColumnCount.Value > _probeCollection.Count)
      {
        ColumnCount.Value = _probeCollection.Count;
      }
    }


    private void ProbeOptionsExecute(object sender, ExecutedRoutedEventArgs e)
    {
      DisplayOptionsWindow();
    }


    private void StartStopExecute(object sender, ExecutedRoutedEventArgs e)
    {
      var toggleStatus = StartStopMenuHeader.Text;

      foreach (var probe in _probeCollection)
      {
        if (toggleStatus == Strings.Toolbar_StopAll && probe.IsActive)
        {
          probe.StartStop();
        }
        else if (toggleStatus == Strings.Toolbar_StartAll && !probe.IsActive)
        {
          probe.StartStop();
        }
      }
    }


    private void HelpExecute(object sender, ExecutedRoutedEventArgs e)
    {
      if (HelpWindow._OpenWindow == null)
      {
        new HelpWindow().Show();
      }
      else
      {
        HelpWindow._OpenWindow.Activate();
      }
    }


    private void NewInstanceExecute(object sender, ExecutedRoutedEventArgs e)
    {
      try
      {
        var p = new System.Diagnostics.Process { StartInfo = { FileName = System.Reflection.Assembly.GetExecutingAssembly().Location } };
        p.Start();
      }

      catch (Exception ex)
      {
        var errorWindow = DialogWindow.ErrorWindow($"{Strings.Error_FailedToLaunch} {ex.Message}");
        errorWindow.Owner = this;
        errorWindow.ShowDialog();
      }
    }


    private void TraceRouteExecute(object sender, ExecutedRoutedEventArgs e)
    {
      new TraceRouteWindow().Show();
    }


    private void FloodHostExecute(object sender, ExecutedRoutedEventArgs e)
    {
      new FloodHostWindow().Show();
    }


    private void AddMonitorExecute(object sender, ExecutedRoutedEventArgs e)
    {
      _probeCollection.Add(new Probe());
    }


    private void mnuProbeOptions_Click(object sender, RoutedEventArgs e)
    {
      DisplayOptionsWindow();
    }


    private void DisplayOptionsWindow()
    {
      if (OptionsWindow.OpenWindow == null)
      // Open the options window.
      {
        new OptionsWindow().Show();
      }
      else
      // Options window is already open.  Activate it.
      {
        OptionsWindow.OpenWindow.Activate();
      }
    }


    private void RemoveAllProbes()
    {
      foreach (var probe in _probeCollection)
      {
        if (probe.IsActive)
        {
          probe.CancelSource.Cancel();
        }
      }

      _probeCollection.Clear();
      Probe.ActiveCount = 0;
    }

    private void LoadFavorites()
    {
      // Clear existing favorites menu.
      for (var i = mnuFavorites.Items.Count - 1; i > 2; --i)
      {
        mnuFavorites.Items.RemoveAt(i);
      }

      // Load favorites.
      foreach (var fav in Favorite.GetTitles())
      {
        var menuItem = new MenuItem { Header = fav };
        menuItem.Click += (s, r) =>
                          {
                            var selectedFavorite = s as MenuItem;
                            StartFavorite(selectedFavorite?.Header.ToString());
                          };

        mnuFavorites.Items.Add(menuItem);
      }
    }

    private void StartFavorite(string selectedFavorite)
    {
      if (string.IsNullOrWhiteSpace(selectedFavorite))
      {
        return;
      }

      RemoveAllProbes();

      var favorite = Favorite.GetContents(selectedFavorite);
      if (favorite.Hostnames.Count < 1)
      {
        AddProbe();
      }
      else
      {
        AddProbe(numberOfProbes: favorite.Hostnames.Count);
        for (var i = 0; i < favorite.Hostnames.Count; ++i)
        {
          _probeCollection[i].Hostname = favorite.Hostnames[i].ToUpper();
          _probeCollection[i].Alias = _aliases.ContainsKey(_probeCollection[i].Hostname)
            ? _aliases[_probeCollection[i].Hostname]
            : null;
          _probeCollection[i].StartStop();
        }
      }

      ColumnCount.Value = favorite.ColumnCount;
    }

    private void LoadAliases()
    {
      _aliases = Alias.GetAliases();
      var aliasList = _aliases.ToList();
      aliasList.Sort((pair1, pair2) => string.Compare(pair1.Value, pair2.Value, StringComparison.Ordinal));

      // Clear existing aliases menu.
      for (var i = mnuAliases.Items.Count - 1; i > 1; --i)
      {
        mnuAliases.Items.RemoveAt(i);
      }

      // Load aliases.
      foreach (var alias in aliasList)
      {
        mnuAliases.Items.Add(BuildAliasMenuItem(alias, false));
      }

      foreach (var probe in _probeCollection)
      {
        probe.Alias = probe.Hostname != null && _aliases.ContainsKey(probe.Hostname)
                        ? _aliases[probe.Hostname]
                        : string.Empty;
      }
    }

    private MenuItem BuildAliasMenuItem(KeyValuePair<string, string> alias, bool isContextMenu)
    {
      var menuItem = new MenuItem { Header = alias.Value };

      if (isContextMenu)
      {
        menuItem.Click += (s, r) =>
                          {
                            if (s is MenuItem selectedMenuItem)
                            {
                              var selectedAlias = (Probe)selectedMenuItem.DataContext;
                              selectedAlias.Hostname = _aliases.FirstOrDefault(x => x.Value == selectedMenuItem.Header.ToString()).Key;
                              selectedAlias.StartStop();
                            }
                          };
      }
      else
      {
        menuItem.Click += (s, r) =>
                          {
                            var selectedAlias = s as MenuItem;
                            if (selectedAlias == null)
                            {
                              return;
                            }

                            var didFindEmptyHost = false;
                            foreach (var probe in _probeCollection)
                            {
                              if (string.IsNullOrWhiteSpace(probe.Hostname))
                              {
                                probe.Hostname = _aliases.FirstOrDefault(x => x.Value == selectedAlias.Header.ToString()).Key;
                                probe.StartStop();
                                didFindEmptyHost = true;
                                break;
                              }
                            }

                            if (!didFindEmptyHost)
                            {
                              AddProbe();
                              _probeCollection[_probeCollection.Count - 1].Hostname = _aliases.FirstOrDefault(x => x.Value == selectedAlias.Header.ToString()).Key;
                              _probeCollection[_probeCollection.Count - 1].StartStop();
                            }
                          };
      }

      return menuItem;
    }


    private void mnuAddToFavorites_Click(object sender, RoutedEventArgs e)
    {
      // Display add to favorites window.
      var currentHostList = new List<string>();
      var haveAnyHostnamesBeenEntered = false;

      foreach (var probe in _probeCollection)
      {
        currentHostList.Add(probe.Hostname);
        if (!string.IsNullOrWhiteSpace(probe.Hostname))
        {
          haveAnyHostnamesBeenEntered = true;
        }
      }

      if (!haveAnyHostnamesBeenEntered)
      {
        var errorWindow = DialogWindow.ErrorWindow(Strings.Error_NoHostsForFavorite);
        errorWindow.Owner = this;
        errorWindow.ShowDialog();
        return;
      }

      var addToFavoritesWindow = new NewFavoriteWindow(currentHostList, (int)ColumnCount.Value) { Owner = this };
      if (addToFavoritesWindow.ShowDialog() == true)
      {
        LoadFavorites();
      }
    }

    private void mnuManageFavorites_Click(object sender, RoutedEventArgs e)
    {
      if (ManageFavoritesWindow.openWindow == null)
      {
        // Open the favorites window.
        var manageFavoritesWindow = new ManageFavoritesWindow { Owner = this };
        manageFavoritesWindow.ShowDialog();
        LoadFavorites();
      }
      else
      {
        // Favorites window is already open.  Activate it.
        ManageFavoritesWindow.openWindow.Activate();
      }
    }

    private void mnuManageAliases_Click(object sender, RoutedEventArgs e)
    {
      if (ManageAliasesWindow.OpenWindow == null)
      {
        // Open the aliases window.
        var manageAliasesWindow = new ManageAliasesWindow { Owner = this };
        manageAliasesWindow.ShowDialog();
        LoadAliases();
      }
      else
      {
        // Aliases window is already open.  Activate it.
        ManageAliasesWindow.OpenWindow.Activate();
      }
    }

    private void PopupAlways_Click(object sender, RoutedEventArgs e)
    {
      PopupAlways.IsChecked = true;
      PopupNever.IsChecked = false;
      PopupWhenMinimized.IsChecked = false;
      ApplicationOptions.PopupOption = ApplicationOptions.PopupNotificationOption.Always;
    }

    private void PopupNever_Click(object sender, RoutedEventArgs e)
    {
      PopupAlways.IsChecked = false;
      PopupNever.IsChecked = true;
      PopupWhenMinimized.IsChecked = false;
      ApplicationOptions.PopupOption = ApplicationOptions.PopupNotificationOption.Never;
    }

    private void PopupWhenMinimized_Click(object sender, RoutedEventArgs e)
    {
      PopupAlways.IsChecked = false;
      PopupNever.IsChecked = false;
      PopupWhenMinimized.IsChecked = true;
      ApplicationOptions.PopupOption = ApplicationOptions.PopupNotificationOption.WhenMinimized;
    }

    private void IsolatedView_Click(object sender, RoutedEventArgs e)
    {
      var probe = (Probe)((Button)sender).DataContext;
      if (probe.IsolatedWindow == null || probe.IsolatedWindow.IsLoaded == false)
      {
        new IsolatedPingWindow(probe).Show();
      }
      else if (probe.IsolatedWindow.IsLoaded)
      {
        probe.IsolatedWindow.Focus();
      }
    }

    private void EditAlias_Click(object sender, RoutedEventArgs e)
    {
      var probe = (sender as Button)?.DataContext as Probe;

      if (string.IsNullOrEmpty(probe?.Hostname))
      {
        return;
      }

      probe.Alias = _aliases.ContainsKey(probe.Hostname) ? _aliases[probe.Hostname] : string.Empty;

      var wnd = new EditAliasWindow(probe) { Owner = this };

      if (wnd.ShowDialog() == true)
      {
        LoadAliases();
      }
    }

    private void mnuStatusHistory_Click(object sender, RoutedEventArgs e)
    {
      if (Probe.StatusWindow == null || Probe.StatusWindow.IsLoaded == false)
      {
        var wnd = new StatusHistoryWindow(Probe.StatusChangeLog);
        Probe.StatusWindow = wnd;
        wnd.Show();
      }
      else if (Probe.StatusWindow.IsLoaded)
      {
        Probe.StatusWindow.Focus();
      }
    }

    private void Hostname_Loaded(object sender, RoutedEventArgs e)
    {
      // Set focus to textbox on newly added monitors.  If the hostname field is blank for any existing monitors, do not change focus.
      for (var i = 0; i < _probeCollection.Count - 1; ++i)
      {
        if (string.IsNullOrEmpty(_probeCollection[i].Hostname))
        {
          return;
        }
      }

      ((TextBox)sender).Focus();
    }

    private void Hostname_TextChanged(object sender, TextChangedEventArgs e)
    {
      // Check if there is an alias for the hostname as you type.
      var probe = (sender as TextBox)?.DataContext as Probe;
      if (probe?.Hostname != null)
      {
        probe.Alias = _aliases.ContainsKey(probe.Hostname) ? _aliases[probe.Hostname] : null;
      }
    }

    private void Window_ContentRendered(object sender, EventArgs e)
    {
      // Set initial focus first text box.
      if (_probeCollection.Count <= 0)
      {
        return;
      }

      var cp = ProbeItemsControl.ItemContainerGenerator.ContainerFromIndex(0) as ContentPresenter;
      var tb = (TextBox)cp?.ContentTemplate.FindName("Hostname", cp);

      tb?.Focus();
    }

    private void Logo_TargetUpdated(object sender, System.Windows.Data.DataTransferEventArgs e)
    {
      // This event is tied to the background image that appears in each probe window.
      // After a probe is started, this event removes the image from the ItemsControl.
      if (!(sender is Image image) || image.Visibility != Visibility.Collapsed)
      {
        return;
      }

      image.Visibility = Visibility.Collapsed;
      image.Source     = null;
    }

    // Experimenting with multi-color listbox items.
    //private void TextBlock_Loaded(object sender, RoutedEventArgs e)
    //{
    //    var tb = sender as TextBlock;
    //    var message = tb.Text;
    //    tb.Text = string.Empty;
    //    foreach (var letter in message)
    //    {
    //        if (char.IsControl(letter)) tb.Inlines.Add(letter.ToString());
    //        else if (char.IsSymbol(letter))
    //            tb.Inlines.Add(new Run(letter.ToString()) { Foreground = Brushes.Red });
    //        else
    //            tb.Inlines.Add(new Run(letter.ToString()) { Foreground = Brushes.Green });
    //    }
    //}
  }
}