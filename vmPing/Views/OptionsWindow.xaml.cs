﻿using System;
using System.IO;
using System.Media;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using vmPing.Classes;

namespace vmPing.Views
{
  /// <summary>
  /// Interaction logic for OptionsWindow.xaml
  /// </summary>
  public partial class OptionsWindow
  {
    private readonly IEmailSender _emailSender;

    public OptionsWindow(IEmailSender emailSender)
    {
      _emailSender = emailSender;
      InitializeComponent();

      PopulateGeneralOptions();
      PopulateEmailAlertOptions();
      PopulateAudioAlertOptions();
      PopulateLogOutputOptions();
      PopulateAdvancedOptions();
      PopulateLayoutOptions();
    }

    public static OptionsWindow OpenWindow;

    private void ShowError(string message, TabItem tabItem, Control control)
    {
      tabItem.Focus();
      var errorWindow = DialogWindow.ErrorWindow(message);
      errorWindow.Owner = this;
      errorWindow.ShowDialog();
      control.Focus();
    }

    private void PopulateGeneralOptions()
    {
      string pingIntervalText;
      int pingIntervalDivisor;
      var pingInterval = ApplicationOptions.PingInterval;
      var pingTimeout = ApplicationOptions.PingTimeout;

      if (ApplicationOptions.PingInterval >= 3600000 && ApplicationOptions.PingInterval % 3600000 == 0)
      {
        pingIntervalText = "Hours";
        pingIntervalDivisor = 3600000;
      }
      else if (ApplicationOptions.PingInterval >= 60000 && ApplicationOptions.PingInterval % 60000 == 0)
      {
        pingIntervalText = "Minutes";
        pingIntervalDivisor = 60000;
      }
      else
      {
        pingIntervalText = "Seconds";
        pingIntervalDivisor = 1000;
      }

      pingInterval /= pingIntervalDivisor;
      pingTimeout /= 1000;

      txtPingInterval.Text = pingInterval.ToString();
      txtPingTimeout.Text = pingTimeout.ToString();
      txtAlertThreshold.Text = ApplicationOptions.AlertThreshold.ToString();
      cboPingInterval.Text = pingIntervalText;
    }

    private void PopulateEmailAlertOptions()
    {
      IsEmailAlertsEnabled.IsChecked = ApplicationOptions.IsEmailAlertEnabled;
      IsSmtpAuthenticationRequired.IsChecked = ApplicationOptions.IsEmailAuthenticationRequired;
      SmtpServer.Text = ApplicationOptions.EmailServer;
      SmtpPort.Text = ApplicationOptions.EmailPort;
      SmtpUsername.Text = ApplicationOptions.EmailUser;
      SmtpPassword.Password = ApplicationOptions.EmailPassword;
      EmailRecipientAddress.Text = ApplicationOptions.EmailRecipient;
      EmailFromAddress.Text = ApplicationOptions.EmailFromAddress;
    }

    private void PopulateAudioAlertOptions()
    {
      IsAudioAlertEnabled.IsChecked = ApplicationOptions.IsAudioAlertEnabled;
      AudioFilePath.Text = ApplicationOptions.AudioFilePath;
    }

    private void PopulateLogOutputOptions()
    {
      LogPath.Text = ApplicationOptions.LogPath;
      IsLogOutputEnabled.IsChecked = ApplicationOptions.IsLogOutputEnabled;
      LogStatusChangesPath.Text = ApplicationOptions.LogStatusChangesPath;
      IsLogStatusChangesEnabled.IsChecked = ApplicationOptions.IsLogStatusChangesEnabled;
    }

    private void PopulateAdvancedOptions()
    {
      TTL.Text = ApplicationOptions.TTL.ToString();
      DontFragment.IsChecked = ApplicationOptions.DontFragment;

      if (ApplicationOptions.UseCustomBuffer)
      {
        UseCustomPacketOption.IsChecked = true;
        PacketData.Text = Encoding.ASCII.GetString(ApplicationOptions.Buffer);
      }
      else
      {
        PacketSizeOption.IsChecked = true;
        PacketSize.Text = ApplicationOptions.Buffer.Length.ToString();
      }

      UpdateByteCount();
    }

    private void PopulateLayoutOptions()
    {
      BackgroundColor_Probe_Inactive.Text = ApplicationOptions.BackgroundColor_Probe_Inactive;
      BackgroundColor_Probe_Up.Text = ApplicationOptions.BackgroundColor_Probe_Up;
      BackgroundColor_Probe_Down.Text = ApplicationOptions.BackgroundColor_Probe_Down;
      BackgroundColor_Probe_Error.Text = ApplicationOptions.BackgroundColor_Probe_Error;
      BackgroundColor_Probe_Indeterminate.Text = ApplicationOptions.BackgroundColor_Probe_Indeterminate;
      ForegroundColor_Probe_Inactive.Text = ApplicationOptions.ForegroundColor_Probe_Inactive;
      ForegroundColor_Probe_Up.Text = ApplicationOptions.ForegroundColor_Probe_Up;
      ForegroundColor_Probe_Down.Text = ApplicationOptions.ForegroundColor_Probe_Down;
      ForegroundColor_Probe_Error.Text = ApplicationOptions.ForegroundColor_Probe_Error;
      ForegroundColor_Probe_Indeterminate.Text = ApplicationOptions.ForegroundColor_Probe_Indeterminate;
      ForegroundColor_Stats_Inactive.Text = ApplicationOptions.ForegroundColor_Stats_Inactive;
      ForegroundColor_Stats_Up.Text = ApplicationOptions.ForegroundColor_Stats_Up;
      ForegroundColor_Stats_Down.Text = ApplicationOptions.ForegroundColor_Stats_Down;
      ForegroundColor_Stats_Error.Text = ApplicationOptions.ForegroundColor_Stats_Error;
      ForegroundColor_Stats_Indeterminate.Text = ApplicationOptions.ForegroundColor_Stats_Inactive;
      ForegroundColor_Alias_Inactive.Text = ApplicationOptions.ForegroundColor_Alias_Inactive;
      ForegroundColor_Alias_Up.Text = ApplicationOptions.ForegroundColor_Alias_Up;
      ForegroundColor_Alias_Down.Text = ApplicationOptions.ForegroundColor_Alias_Down;
      ForegroundColor_Alias_Error.Text = ApplicationOptions.ForegroundColor_Alias_Error;
      ForegroundColor_Alias_Indeterminate.Text = ApplicationOptions.ForegroundColor_Alias_Indeterminate;
    }

    private void btnOk_Click(object sender, RoutedEventArgs e)
    {
      if (SaveGeneralOptions() == false)
      {
        return;
      }

      if (SaveEmailAlertOptions() == false)
      {
        return;
      }

      if (SaveAudioAlertOptions() == false)
      {
        return;
      }

      if (SaveLogOutputOptions() == false)
      {
        return;
      }

      if (SaveAdvancedOptions() == false)
      {
        return;
      }

      if (SaveLayoutOptions() == false)
      {
        return;
      }

      if (SaveAsDefaults.IsChecked == true)
      {
        Configuration.WriteConfigurationOptions();
      }

      Close();
    }

    private bool SaveGeneralOptions()
    {
      if (txtPingInterval.Text.Length == 0)
      {
        ShowError("Please enter a valid ping interval.", GeneralTab, txtPingInterval);
        return false;
      }
      else if (txtPingTimeout.Text.Length == 0)
      {
        ShowError("Please enter a valid ping timeout.", GeneralTab, txtPingTimeout);
        return false;
      }
      else if (txtAlertThreshold.Text.Length == 0)
      {
        ShowError("Please enter a valid alert threshold.", GeneralTab, txtAlertThreshold);
        return false;
      }


      // Ping interval
      var multiplier = 1000;

      switch (cboPingInterval.Text)
      {
        case "Seconds":
          multiplier = 1000;
          break;
        case "Minutes":
          multiplier = 1000 * 60;
          break;
        case "Hours":
          multiplier = 1000 * 60 * 60;
          break;
      }

      if (int.TryParse(txtPingInterval.Text, out var pingInterval) && pingInterval > 0 && pingInterval <= 86400)
      {
        pingInterval *= multiplier;
      }
      else
      {
        pingInterval = Constants.DefaultInterval;
      }

      ApplicationOptions.PingInterval = pingInterval;

      // Ping timeout

      if (int.TryParse(txtPingTimeout.Text, out var pingTimeout) && pingTimeout > 0 && pingTimeout <= 60)
      {
        pingTimeout *= 1000;
      }
      else
      {
        pingTimeout = Constants.DefaultTimeout;
      }

      ApplicationOptions.PingTimeout = pingTimeout;

      // Alert threshold

      var isThresholdValid = int.TryParse(txtAlertThreshold.Text, out var alertThreshold) && alertThreshold > 0 && alertThreshold <= 60;
      if (!isThresholdValid)
      {
        alertThreshold = 1;
      }

      ApplicationOptions.AlertThreshold = alertThreshold;

      return true;
    }

    private bool SaveAdvancedOptions()
    {
      // Validate input.

      var regex = new Regex("^\\d+$");

      // Validate TTL.
      if (!regex.IsMatch(TTL.Text) || int.Parse(TTL.Text) < 1 || int.Parse(TTL.Text) > 255)
      {
        ShowError("Please enter a valid TTL between 1 and 255.", AdvancedTab, TTL);
        return false;
      }

      // Apply TTL.
      ApplicationOptions.TTL = int.Parse(TTL.Text);

      // Validate packet size.
      if (PacketSizeOption.IsChecked == true)
      {
        if (!regex.IsMatch(PacketSize.Text) || int.Parse(PacketSize.Text) < 0 || int.Parse(PacketSize.Text) > 65500)
        {
          ShowError("Please enter a valid data size between 0 and 65,500.", AdvancedTab, PacketSize);
          return false;
        }

        // Apply packet size.
        ApplicationOptions.Buffer = new byte[int.Parse(PacketSize.Text)];
        ApplicationOptions.UseCustomBuffer = false;

        // Fill buffer with default text.
        if (ApplicationOptions.Buffer.Length >= 33)
        {
          Buffer.BlockCopy(Encoding.ASCII.GetBytes(Constants.DefaultIcmpData), 0, ApplicationOptions.Buffer, 0, 33);
        }
      }
      else
      {
        // Use custom packet data.
        ApplicationOptions.Buffer = Encoding.ASCII.GetBytes(PacketData.Text);
        ApplicationOptions.UseCustomBuffer = true;
      }

      // Apply fragment / don't fragment option.
      ApplicationOptions.DontFragment = DontFragment.IsChecked == true;

      // Update ping options (TTL / Don't fragment settings)
      ApplicationOptions.UpdatePingOptions();

      return true;
    }

    private bool SaveEmailAlertOptions()
    {
      // Validate input.
      if (IsEmailAlertsEnabled.IsChecked == true)
      {
        var regex = new Regex("^\\d+$");

        if (SmtpServer.Text.Length == 0)
        {
          ShowError("Please enter a valid address for your outgoing mail server.", EmailAlertsTab, SmtpServer);
          return false;
        }

        if (SmtpPort.Text.Length == 0 || !regex.IsMatch(SmtpPort.Text))
        {
          ShowError("Please enter a valid port number for your SMTP server.", EmailAlertsTab, SmtpPort);
          return false;
        }

        if (EmailRecipientAddress.Text.Length == 0)
        {
          ShowError("Please enter a valid recipient email address.  This is the address that will receive alerts.", EmailAlertsTab, EmailRecipientAddress);
          return false;
        }

        if (EmailFromAddress.Text.Length == 0)
        {
          ShowError("Please enter a valid 'from' address.  This address will appear as the sender for any alerts that are sent.", EmailAlertsTab, EmailFromAddress);
          return false;
        }

        if (IsSmtpAuthenticationRequired.IsChecked == true)
        {
          ApplicationOptions.IsEmailAuthenticationRequired = true;
          if (SmtpUsername.Text.Length == 0)
          {
            ShowError("Please enter a valid username for your mail server.", EmailAlertsTab, SmtpUsername);
            return false;
          }
        }
        else
        {
          ApplicationOptions.IsEmailAuthenticationRequired = false;
          SmtpUsername.Text = string.Empty;
          SmtpPassword.Password = string.Empty;
        }

        ApplicationOptions.IsEmailAlertEnabled = true;
        ApplicationOptions.EmailServer = SmtpServer.Text;
        ApplicationOptions.EmailPort = SmtpPort.Text;
        ApplicationOptions.EmailUser = SmtpUsername.Text;
        ApplicationOptions.EmailPassword = SmtpPassword.Password;
        ApplicationOptions.EmailRecipient = EmailRecipientAddress.Text;
        ApplicationOptions.EmailFromAddress = EmailFromAddress.Text;

        if (IsSmtpAuthenticationRequired.IsChecked == true && SaveAsDefaults.IsChecked == true)
        {
          MessageBox.Show(
                          "You have chosen to save your SMTP credentials to disk." + Environment.NewLine + Environment.NewLine +
                          "While the data is stored in an encrypted format, anyone with access to your vmPing configuration file can decrypt the data.",
                          "vmPing Warning",
                          MessageBoxButton.OK,
                          MessageBoxImage.Warning);
        }

        return true;
      }
      else
      {
        ApplicationOptions.IsEmailAlertEnabled = false;
        return true;
      }
    }

    private bool SaveAudioAlertOptions()
    {
      if (IsAudioAlertEnabled.IsChecked == true)
      {
        try
        {
          if (Path.GetFileName(AudioFilePath.Text).IndexOfAny(Path.GetInvalidFileNameChars()) >= 0 ||
              !Directory.Exists(Path.GetDirectoryName(AudioFilePath.Text)) ||
              Path.GetFileName(AudioFilePath.Text).Length < 1)
          {
            throw new Exception();
          }
        }
        catch
        {
          ShowError("The specified path does not exist.  Please enter a valid path.", AudioAlertTab, AudioFilePath);
          return false;
        }

        ApplicationOptions.IsAudioAlertEnabled = true;
        ApplicationOptions.AudioFilePath = AudioFilePath.Text;
      }
      else
      {
        ApplicationOptions.IsAudioAlertEnabled = false;
      }

      return true;
    }

    private bool SaveLogOutputOptions()
    {
      if (IsLogOutputEnabled.IsChecked == true)
      {
        if (!Directory.Exists(LogPath.Text))
        {
          ShowError("The specified path does not exist.  Please enter a valid path.", LogOutputTab, LogPath);
          return false;
        }

        ApplicationOptions.IsLogOutputEnabled = true;
        ApplicationOptions.LogPath = LogPath.Text;
      }
      else
      {
        ApplicationOptions.IsLogOutputEnabled = false;
      }

      if (IsLogStatusChangesEnabled.IsChecked == true)
      {
        try
        {
          if (Path.GetFileName(LogStatusChangesPath.Text).IndexOfAny(Path.GetInvalidFileNameChars()) >= 0 ||
              !Directory.Exists(Path.GetDirectoryName(LogStatusChangesPath.Text)) ||
              Path.GetFileName(LogStatusChangesPath.Text).Length < 1)
          {
            throw new Exception();
          }
        }
        catch
        {
          ShowError("The specified path does not exist.  Please enter a valid path.", LogOutputTab, LogStatusChangesPath);
          return false;
        }

        ApplicationOptions.IsLogStatusChangesEnabled = true;
        ApplicationOptions.LogStatusChangesPath = LogStatusChangesPath.Text;
      }
      else
      {
        ApplicationOptions.IsLogStatusChangesEnabled = false;
      }

      return true;
    }

    private bool SaveLayoutOptions()
    {
      // Validate input.
      foreach (var control in ColorsDockPanel.GetChildren())
      {
        if (!(control is TextBox textBox))
        {
          continue;
        }

        if (Util.IsValidHtmlColor(textBox.Text))
        {
          continue;
        }

        ShowError("Please enter a valid HTML color code.  Accepted formats are #RGB, #RRGGBB, and #AARRGGBB.  Example: #3266CF", LayoutTab, textBox);
        textBox.SelectAll();

        return false;
      }

      ApplicationOptions.BackgroundColor_Probe_Inactive = BackgroundColor_Probe_Inactive.Text;
      ApplicationOptions.BackgroundColor_Probe_Up = BackgroundColor_Probe_Up.Text;
      ApplicationOptions.BackgroundColor_Probe_Down = BackgroundColor_Probe_Down.Text;
      ApplicationOptions.BackgroundColor_Probe_Indeterminate = BackgroundColor_Probe_Indeterminate.Text;
      ApplicationOptions.BackgroundColor_Probe_Error = BackgroundColor_Probe_Error.Text;
      ApplicationOptions.ForegroundColor_Probe_Inactive = ForegroundColor_Probe_Inactive.Text;
      ApplicationOptions.ForegroundColor_Probe_Up = ForegroundColor_Probe_Up.Text;
      ApplicationOptions.ForegroundColor_Probe_Down = ForegroundColor_Probe_Down.Text;
      ApplicationOptions.ForegroundColor_Probe_Indeterminate = ForegroundColor_Probe_Indeterminate.Text;
      ApplicationOptions.ForegroundColor_Probe_Error = ForegroundColor_Probe_Error.Text;
      ApplicationOptions.ForegroundColor_Stats_Inactive = ForegroundColor_Stats_Inactive.Text;
      ApplicationOptions.ForegroundColor_Stats_Up = ForegroundColor_Stats_Up.Text;
      ApplicationOptions.ForegroundColor_Stats_Down = ForegroundColor_Stats_Down.Text;
      ApplicationOptions.ForegroundColor_Stats_Indeterminate = ForegroundColor_Stats_Indeterminate.Text;
      ApplicationOptions.ForegroundColor_Stats_Error = ForegroundColor_Stats_Error.Text;
      ApplicationOptions.ForegroundColor_Alias_Inactive = ForegroundColor_Alias_Inactive.Text;
      ApplicationOptions.ForegroundColor_Alias_Up = ForegroundColor_Alias_Up.Text;
      ApplicationOptions.ForegroundColor_Alias_Down = ForegroundColor_Alias_Down.Text;
      ApplicationOptions.ForegroundColor_Alias_Indeterminate = ForegroundColor_Alias_Indeterminate.Text;
      ApplicationOptions.ForegroundColor_Alias_Error = ForegroundColor_Alias_Error.Text;

      return true;
    }

    private void txtNumericTextbox_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
      var regex = new Regex("[^0-9.-]+");
      if (regex.IsMatch(e.Text))
      {
        e.Handled = true;
      }
    }

    private void HtmlColor_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
      var regex = new Regex("[#a-fA-F0-9]");
      if (!regex.IsMatch(e.Text))
      {
        e.Handled = true;
      }
    }

    private void btnCancel_Click(object sender, RoutedEventArgs e)
    {
      Close();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
      OpenWindow = this;
    }

    private void Window_Closed(object sender, EventArgs e)
    {
      OpenWindow = null;
    }

    private void EmailRecipientAddress_LostFocus(object sender, RoutedEventArgs e)
    {
      if (EmailFromAddress.Text.Length == 0 && EmailRecipientAddress.Text.IndexOf('@') >= 0)
      {
        EmailFromAddress.Text = "vmPing" + EmailRecipientAddress.Text.Substring(EmailRecipientAddress.Text.IndexOf('@'));
      }
    }

    private void IsEmailAlertsEnabled_Click(object sender, RoutedEventArgs e)
    {
      if (IsEmailAlertsEnabled.IsChecked == true && SmtpServer.Text.Length == 0)
      {
        SmtpServer.Focus();
      }
    }

    private void IsSmtpAuthenticationRequired_Click(object sender, RoutedEventArgs e)
    {
      if (IsSmtpAuthenticationRequired.IsChecked == true)
      {
        SmtpUsername.Focus();
      }
    }

    private async void TestEmail_Click(object sender, RoutedEventArgs e)
    {
      TestEmailButton.IsEnabled = false;
      TestEmailButton.Content = "Testing...";
      var serverAddress  = SmtpServer.Text;
      var serverPort     = SmtpPort.Text;
      var isAuthRequired = IsSmtpAuthenticationRequired.IsChecked == true;
      var username       = SmtpUsername.Text;
      var password       = SmtpPassword.SecurePassword;
      var mailFrom       = EmailFromAddress.Text;
      var mailRecipient  = EmailRecipientAddress.Text;

      await Task.Run(() =>
      {
        try
        {
          _emailSender.SendTestEmail(serverAddress, serverPort, isAuthRequired, username, password, mailFrom, mailRecipient);
        }
        catch (SmtpException sex)
        {
          var exMessage = sex.Message;
          if (sex.InnerException is WebException wex)
          {
            exMessage = wex.Message;
          }

          Application.Current.Dispatcher.BeginInvoke(new Action(() => ShowError(exMessage, EmailAlertsTab, TestEmailButton)));
        }
        catch (Exception ex)
        {
          Application.Current.Dispatcher.BeginInvoke(new Action(() => ShowError(ex.Message, EmailAlertsTab, TestEmailButton)));
        }
      });
      TestEmailButton.IsEnabled = true;
      TestEmailButton.Content = "Test";
    }

    private void BrowseLogPath_Click(object sender, RoutedEventArgs e)
    {
      using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
      {
        dialog.Description = vmPing.Properties.Strings.SelectLocationForLogFiles_;
        var result = dialog.ShowDialog();

        if (result == System.Windows.Forms.DialogResult.OK)
        {
          LogPath.Text = dialog.SelectedPath;
        }
      }
    }

    private void BrowseLogStatusChangesPath_Click(object sender, RoutedEventArgs e)
    {
      using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
      {
        dialog.Description = vmPing.Properties.Strings.SelectLocationForLogFiles_;
        var result = dialog.ShowDialog();

        if (result == System.Windows.Forms.DialogResult.OK)
        {
          LogStatusChangesPath.Text = Path.Combine(dialog.SelectedPath, "vmping-status.txt");
        }
      }
    }

    private void AudioFilePath_Click(object sender, RoutedEventArgs e)
    {
      const string wavFilesFilter = "WAV files (*.wav)|*.wav|All files|*.*";

      using (var audiofileDialog = new System.Windows.Forms.OpenFileDialog())
      {
        audiofileDialog.Title            = vmPing.Properties.Strings.SelectAudioFile;
        audiofileDialog.RestoreDirectory = true;
        audiofileDialog.Multiselect      = false;
        audiofileDialog.Filter           = wavFilesFilter;
        audiofileDialog.DefaultExt       = ".wav";

        if (audiofileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        {
          AudioFilePath.Text = audiofileDialog.FileName;
        }
      }
    }

    private void AudioFileTest_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        using (var player = new SoundPlayer(AudioFilePath.Text))
        {
          player.Play();
        }
      }
      catch
      {
        ShowError("Unable to play audio file.", AudioAlertTab, AudioFilePath);
      }
    }

    private void UpdateByteCount()
    {
      var regex = new Regex("^\\d+$");
      if (PacketSizeOption.IsChecked == true)
      {
        if (PacketSize != null && regex.IsMatch(PacketSize.Text))
        {
          Bytes.Text = (int.Parse(PacketSize.Text) + 28).ToString();
        }
        else
        {
          Bytes.Text = "?";
        }
      }
      else
      {
        Bytes.Text = (PacketData.Text.Length + 28).ToString();
      }
    }

    private void PacketData_TextChanged(object sender, TextChangedEventArgs e)
    {
      UpdateByteCount();
    }

    private void PacketSizeOption_Checked(object sender, RoutedEventArgs e)
    {
      if (IsLoaded)
      {
        UpdateByteCount();
      }
    }

    private void UseCustomPacketOption_Checked(object sender, RoutedEventArgs e)
    {
      if (IsLoaded)
      {
        UpdateByteCount();
      }
    }

    private void RestoreDefaultColors_Click(object sender, RoutedEventArgs e)
    {
      BackgroundColor_Probe_Inactive.Text = Constants.Color_Probe_Background_Inactive;
      BackgroundColor_Probe_Up.Text = Constants.Color_Probe_Background_Up;
      BackgroundColor_Probe_Down.Text = Constants.Color_Probe_Background_Down;
      BackgroundColor_Probe_Error.Text = Constants.Color_Probe_Background_Error;
      BackgroundColor_Probe_Indeterminate.Text = Constants.Color_Probe_Background_Indeterminate;
      ForegroundColor_Probe_Inactive.Text = Constants.Color_Probe_Foreground_Inactive;
      ForegroundColor_Probe_Up.Text = Constants.Color_Probe_Foreground_Up;
      ForegroundColor_Probe_Down.Text = Constants.Color_Probe_Foreground_Down;
      ForegroundColor_Probe_Error.Text = Constants.Color_Probe_Foreground_Error;
      ForegroundColor_Probe_Indeterminate.Text = Constants.Color_Probe_Foreground_Indeterminate;
      ForegroundColor_Stats_Inactive.Text = Constants.Color_Statistics_Foreground_Inactive;
      ForegroundColor_Stats_Up.Text = Constants.Color_Statistics_Foreground_Up;
      ForegroundColor_Stats_Down.Text = Constants.Color_Statistics_Foreground_Down;
      ForegroundColor_Stats_Error.Text = Constants.Color_Statistics_Foreground_Error;
      ForegroundColor_Stats_Indeterminate.Text = Constants.Color_Statistics_Foreground_Inactive;
      ForegroundColor_Alias_Inactive.Text = Constants.Color_Alias_Foreground_Inactive;
      ForegroundColor_Alias_Up.Text = Constants.Color_Alias_Foreground_Up;
      ForegroundColor_Alias_Down.Text = Constants.Color_Alias_Foreground_Down;
      ForegroundColor_Alias_Error.Text = Constants.Color_Alias_Foreground_Error;
      ForegroundColor_Alias_Indeterminate.Text = Constants.Color_Alias_Foreground_Indeterminate;
    }
  }
}
