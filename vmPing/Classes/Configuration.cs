using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Collections;
using vmPing.Properties;

namespace vmPing.Classes
{
  public class Configuration
  {
    public static string Path = Environment.ExpandEnvironmentVariables(@"%LOCALAPPDATA%\vmPing\vmPing.xml");
    public static string ParentFolder = Environment.ExpandEnvironmentVariables(@"%LOCALAPPDATA%\vmPing");
    public static string OldPath = Environment.ExpandEnvironmentVariables(@"%LOCALAPPDATA%\vmPing\vmPingFavorites.xml");

    public static bool Exists()
    {
      return File.Exists(Path);
    }

    public static bool IsReady()
    {
      if (!Directory.Exists(ParentFolder))
      {
        try
        {
          Directory.CreateDirectory(ParentFolder);
        }
        catch (Exception ex)
        {
          Util.ShowError($"{Strings.Error_CreateDirectory} {ex.Message}");
          return false;
        }
      }

      if (!File.Exists(Path))
      {
        try
        {
          // Generate new xml configuration file.
          var xmlFile = new XmlDocument();
          var rootNode = xmlFile.CreateElement("vmping");
          xmlFile.AppendChild(rootNode);

          rootNode.AppendChild(xmlFile.CreateElement("aliases"));
          rootNode.AppendChild(xmlFile.CreateElement("configuration"));
          rootNode.AppendChild(xmlFile.CreateElement("favorites"));

          xmlFile.Save(Path);
        }
        catch (Exception ex)
        {
          Util.ShowError($"{Strings.Error_CreateConfig} {ex.Message}");
          return false;
        }
      }

      return true;
    }

    public static void UpgradeConfigurationFile()
    {
      if (!Directory.Exists(ParentFolder))
      {
        return;
      }

      if (File.Exists(Path))
      {
        return;
      }

      if (File.Exists(OldPath))
      {
        try
        {
          // Upgrade old configuration file.
          var newXmlFile = new XmlDocument();
          var newRootNode = newXmlFile.CreateElement("vmping");
          newXmlFile.AppendChild(newRootNode);

          var oldXmlFile = new XmlDocument();
          oldXmlFile.Load(OldPath);
          var oldRootNode = oldXmlFile.FirstChild;

          newRootNode.AppendChild(newXmlFile.CreateElement("aliases"));
          newRootNode.AppendChild(newXmlFile.CreateElement("configuration"));
          newRootNode.AppendChild(newXmlFile.ImportNode(oldRootNode, true));

          newXmlFile.Save(Path);
        }
        catch (Exception ex)
        {
          Util.ShowError($"{Strings.Error_UpgradeConfig} {ex.Message}");
        }
      }
    }

    public static string GetEscapedXpath(string xpath)
    {
      if (!xpath.Contains("'"))
      {
        return '\'' + xpath + '\'';
      }
      else if (!xpath.Contains("\""))
      {
        return '"' + xpath + '"';
      }
      else
      {
        return "concat('" + xpath.Replace("'", "',\"'\",'") + "')";
      }
    }

    public static void WriteConfigurationOptions()
    {
      if (IsReady() == false)
      {
        return;
      }

      try
      {
        var xd = new XmlDocument();
        xd.Load(Configuration.Path);

        // Check if configuration node already exists.  If so, delete it.
        var nodeRoot = xd.SelectSingleNode("/vmping");
        foreach (XmlNode node in xd.SelectNodes("/vmping/configuration"))
        {
          nodeRoot.RemoveChild(node);
        }

        // Check if colors node already exists.  If so, delete it.
        foreach (XmlNode node in xd.SelectNodes("/vmping/colors"))
        {
          nodeRoot.RemoveChild(node);
        }

        var configuration = GenerateConfigurationNode(xd);
        var colors = GenerateColorsNode(xd);

        nodeRoot.AppendChild(configuration);
        nodeRoot.AppendChild(colors);
        xd.Save(Configuration.Path);
      }

      catch (Exception ex)
      {
        Util.ShowError($"{Strings.Error_WriteConfig} {ex.Message}");
      }
    }

    private static XmlElement GenerateColorsNode(XmlDocument xd)
    {
      var colors = xd.CreateElement("colors");

      // Write probe background colors.
      colors.AppendChild(GenerateOptionNode(xmlDocument: xd, name: "Probe.Background.Inactive", value: ApplicationOptions.BackgroundColor_Probe_Inactive));
      colors.AppendChild(GenerateOptionNode(xmlDocument: xd, name: "Probe.Background.Up", value: ApplicationOptions.BackgroundColor_Probe_Up));
      colors.AppendChild(GenerateOptionNode(xmlDocument: xd, name: "Probe.Background.Down", value: ApplicationOptions.BackgroundColor_Probe_Down));
      colors.AppendChild(GenerateOptionNode(xmlDocument: xd, name: "Probe.Background.Indeterminate", value: ApplicationOptions.BackgroundColor_Probe_Indeterminate));
      colors.AppendChild(GenerateOptionNode(xmlDocument: xd, name: "Probe.Background.Error", value: ApplicationOptions.BackgroundColor_Probe_Error));

      // Write probe foreground colors.
      colors.AppendChild(GenerateOptionNode(xmlDocument: xd, name: "Probe.Foreground.Inactive", value: ApplicationOptions.ForegroundColor_Probe_Inactive));
      colors.AppendChild(GenerateOptionNode(xmlDocument: xd, name: "Probe.Foreground.Up", value: ApplicationOptions.ForegroundColor_Probe_Up));
      colors.AppendChild(GenerateOptionNode(xmlDocument: xd, name: "Probe.Foreground.Down", value: ApplicationOptions.ForegroundColor_Probe_Down));
      colors.AppendChild(GenerateOptionNode(xmlDocument: xd, name: "Probe.Foreground.Indeterminate", value: ApplicationOptions.ForegroundColor_Probe_Indeterminate));
      colors.AppendChild(GenerateOptionNode(xmlDocument: xd, name: "Probe.Foreground.Error", value: ApplicationOptions.ForegroundColor_Probe_Error));

      // Write statistics foreground colors.
      colors.AppendChild(GenerateOptionNode(xmlDocument: xd, name: "Statistics.Foreground.Inactive", value: ApplicationOptions.ForegroundColor_Stats_Inactive));
      colors.AppendChild(GenerateOptionNode(xmlDocument: xd, name: "Statistics.Foreground.Up", value: ApplicationOptions.ForegroundColor_Stats_Up));
      colors.AppendChild(GenerateOptionNode(xmlDocument: xd, name: "Statistics.Foreground.Down", value: ApplicationOptions.ForegroundColor_Stats_Down));
      colors.AppendChild(GenerateOptionNode(xmlDocument: xd, name: "Statistics.Foreground.Indeterminate", value: ApplicationOptions.ForegroundColor_Stats_Indeterminate));
      colors.AppendChild(GenerateOptionNode(xmlDocument: xd, name: "Statistics.Foreground.Error", value: ApplicationOptions.ForegroundColor_Stats_Error));

      // Write alias foreground colors.
      colors.AppendChild(GenerateOptionNode(xmlDocument: xd, name: "Alias.Foreground.Inactive", value: ApplicationOptions.ForegroundColor_Alias_Inactive));
      colors.AppendChild(GenerateOptionNode(xmlDocument: xd, name: "Alias.Foreground.Up", value: ApplicationOptions.ForegroundColor_Alias_Up));
      colors.AppendChild(GenerateOptionNode(xmlDocument: xd, name: "Alias.Foreground.Down", value: ApplicationOptions.ForegroundColor_Alias_Down));
      colors.AppendChild(GenerateOptionNode(xmlDocument: xd, name: "Alias.Foreground.Indeterminate", value: ApplicationOptions.ForegroundColor_Alias_Indeterminate));
      colors.AppendChild(GenerateOptionNode(xmlDocument: xd, name: "Alias.Foreground.Error", value: ApplicationOptions.ForegroundColor_Alias_Error));

      return colors;
    }

    private static XmlElement GenerateConfigurationNode(XmlDocument xd)
    {
      var configuration = xd.CreateElement("configuration");
      configuration.AppendChild(GenerateOptionNode(xmlDocument: xd, name: "PingInterval", value: ApplicationOptions.PingInterval.ToString()));
      configuration.AppendChild(GenerateOptionNode(xmlDocument: xd, name: "PingTimeout", value: ApplicationOptions.PingTimeout.ToString()));
      configuration.AppendChild(GenerateOptionNode(xmlDocument: xd, name: "TTL", value: ApplicationOptions.TTL.ToString()));
      configuration.AppendChild(GenerateOptionNode(xmlDocument: xd, name: "DontFragment", value: ApplicationOptions.DontFragment.ToString()));
      configuration.AppendChild(GenerateOptionNode(xmlDocument: xd, name: "UseCustomBuffer", value: ApplicationOptions.UseCustomBuffer.ToString()));
      configuration.AppendChild(GenerateOptionNode(xmlDocument: xd, name: "Buffer", value: Encoding.ASCII.GetString(ApplicationOptions.Buffer)));
      configuration.AppendChild(GenerateOptionNode(xmlDocument: xd, name: "AlertThreshold", value: ApplicationOptions.AlertThreshold.ToString()));
      configuration.AppendChild(GenerateOptionNode(xmlDocument: xd, name: "IsEmailAlertEnabled", value: ApplicationOptions.IsEmailAlertEnabled.ToString()));
      configuration.AppendChild(GenerateOptionNode(xmlDocument: xd, name: "EmailServer", value: ApplicationOptions.EmailServer ?? string.Empty));
      configuration.AppendChild(GenerateOptionNode(xmlDocument: xd, name: "EmailPort", value: ApplicationOptions.EmailPort ?? string.Empty));
      configuration.AppendChild(GenerateOptionNode(xmlDocument: xd, name: "IsEmailAuthenticationRequired", value: ApplicationOptions.IsEmailAuthenticationRequired.ToString()));

      configuration.AppendChild(GenerateOptionNode(xmlDocument: xd, name: "EmailUser", value: !string.IsNullOrWhiteSpace(ApplicationOptions.EmailUser) ? Util.EncryptStringAES(ApplicationOptions.EmailUser) : string.Empty));
      configuration.AppendChild(GenerateOptionNode(xmlDocument: xd, name: "EmailPassword", value: !string.IsNullOrWhiteSpace(ApplicationOptions.EmailPassword) ? Util.EncryptStringAES(ApplicationOptions.EmailPassword) : string.Empty));
      configuration.AppendChild(GenerateOptionNode(xmlDocument: xd, name: "EmailRecipient", value: ApplicationOptions.EmailRecipient ?? string.Empty));
      configuration.AppendChild(GenerateOptionNode(xmlDocument: xd, name: "EmailFromAddress", value: ApplicationOptions.EmailFromAddress ?? string.Empty));
      configuration.AppendChild(GenerateOptionNode(xmlDocument: xd, name: "IsAudioAlertEnabled", value: ApplicationOptions.IsAudioAlertEnabled.ToString()));
      configuration.AppendChild(GenerateOptionNode(xmlDocument: xd, name: "AudioFilePath", value: ApplicationOptions.AudioFilePath ?? string.Empty));
      configuration.AppendChild(GenerateOptionNode(xmlDocument: xd, name: "IsLogOutputEnabled", value: ApplicationOptions.IsLogOutputEnabled.ToString()));
      configuration.AppendChild(GenerateOptionNode(xmlDocument: xd, name: "LogPath", value: ApplicationOptions.LogPath ?? string.Empty));
      configuration.AppendChild(GenerateOptionNode(xmlDocument: xd, name: "IsLogStatusChangesEnabled", value: ApplicationOptions.IsLogStatusChangesEnabled.ToString()));
      configuration.AppendChild(GenerateOptionNode(xmlDocument: xd, name: "LogStatusChangesPath", value: ApplicationOptions.LogStatusChangesPath ?? string.Empty));

      return configuration;
    }

    private static XmlElement GenerateOptionNode(XmlDocument xmlDocument, string name, string value)
    {
      var option = xmlDocument.CreateElement("option");
      option.SetAttribute("name", name);
      option.InnerText = value;

      return option;
    }

    public static void Load()
    {
      if (!Exists())
      {
        return;
      }

      try
      {
        var xd = new XmlDocument();
        xd.Load(Path);

        LoadConfigurationNode(xd.SelectNodes("/vmping/configuration/option"));
        LoadColorsNode(xd.SelectNodes("/vmping/colors/option"));
      }

      catch (Exception ex)
      {
        Util.ShowError($"{Strings.Error_LoadConfig} {ex.Message}");
      }
    }

    private static void LoadColorsNode(IEnumerable nodeList)
    {
      void ReadColors(IReadOnlyDictionary<string, string> optionList, string name, Action<string> setApplicationOption)
      {
        if (optionList.TryGetValue(name, out var optionValue))
        {
          if (Util.IsValidHtmlColor(optionValue))
          {
            setApplicationOption(optionValue);
          }
        }
      }

      var options = new Dictionary<string, string>();

      ReadNodesFromConfiguration(nodeList, options);

      // Load probe backgorund colors.
      ReadColors(options, "Probe.Background.Inactive", value => ApplicationOptions.BackgroundColor_Probe_Inactive = value);
      ReadColors(options, "Probe.Background.Up", value => ApplicationOptions.BackgroundColor_Probe_Up = value);
      ReadColors(options, "Probe.Background.Down", value => ApplicationOptions.BackgroundColor_Probe_Down = value);
      ReadColors(options, "Probe.Background.Indeterminate", value => ApplicationOptions.BackgroundColor_Probe_Indeterminate = value);
      ReadColors(options, "Probe.Background.Error", value => ApplicationOptions.BackgroundColor_Probe_Error = value);

      // Load probe foreground colors.
      ReadColors(options, "Probe.Foreground.Inactive", value => ApplicationOptions.ForegroundColor_Probe_Inactive = value);
      ReadColors(options, "Probe.Foreground.Up", value => ApplicationOptions.ForegroundColor_Probe_Up = value);
      ReadColors(options, "Probe.Foreground.Down", value => ApplicationOptions.ForegroundColor_Probe_Down = value);
      ReadColors(options, "Probe.Foreground.Indeterminate", value => ApplicationOptions.ForegroundColor_Probe_Indeterminate = value);
      ReadColors(options, "Probe.Foreground.Error", value => ApplicationOptions.ForegroundColor_Probe_Error = value);

      // Load statistics foreground colors.
      ReadColors(options, "Statistics.Foreground.Inactive", value => ApplicationOptions.ForegroundColor_Stats_Inactive = value);
      ReadColors(options, "Statistics.Foreground.Up", value => ApplicationOptions.ForegroundColor_Stats_Up = value);
      ReadColors(options, "Statistics.Foreground.Down", value => ApplicationOptions.ForegroundColor_Stats_Down = value);
      ReadColors(options, "Statistics.Foreground.Indeterminate", value => ApplicationOptions.ForegroundColor_Stats_Indeterminate = value);
      ReadColors(options, "Statistics.Foreground.Error", value => ApplicationOptions.ForegroundColor_Stats_Error = value);

      // Load alias foreground colors.
      ReadColors(options, "Alias.Foreground.Inactive", value => ApplicationOptions.ForegroundColor_Alias_Inactive = value);
      ReadColors(options, "Alias.Foreground.Up", value => ApplicationOptions.ForegroundColor_Alias_Up = value);
      ReadColors(options, "Alias.Foreground.Down", value => ApplicationOptions.ForegroundColor_Alias_Down = value);
      ReadColors(options, "Alias.Foreground.Indeterminate", value => ApplicationOptions.ForegroundColor_Alias_Indeterminate = value);
      ReadColors(options, "Alias.Foreground.Error", value => ApplicationOptions.ForegroundColor_Alias_Error = value);
    }

    private static void LoadConfigurationNode(IEnumerable nodeList)
    {
      void ReadStringValue(IReadOnlyDictionary<string, string> optionList, string name, Action<string> setApplicationOption)
      {
        if (optionList.TryGetValue(name, out var optionValue))
        {
          setApplicationOption(optionValue);
        }
      }

      void ReadIntValue(IReadOnlyDictionary<string, string> optionList, string name, Action<int> setApplicationOption)
      {
        if (optionList.TryGetValue(name, out var optionValue) && int.TryParse(optionValue, out var value))
        {
          setApplicationOption(value);
        }
      }

      void ReadBoolValue(IReadOnlyDictionary<string, string> optionList, string name, Action<bool> setApplicationOption)
      {
        if (optionList.TryGetValue(name, out var optionValue) && bool.TryParse(optionValue, out var value))
        {
          setApplicationOption(value);
        }
      }

      void ReadBytesValue(IReadOnlyDictionary<string, string> optionList, string name, Action<byte[]> setApplicationOption)
      {
        if (optionList.TryGetValue(name, out var optionValue))
        {
          setApplicationOption(Encoding.ASCII.GetBytes(optionValue));
        }
      }

      void ReadEncryptedValue(IReadOnlyDictionary<string, string> optionList, string name, Action<string> setApplicationOption)
      {
        if (optionList.TryGetValue(name, out var optionValue))
        {
          if (optionValue.Length > 0)
          {
            setApplicationOption(Util.DecryptStringAES(optionValue));
          }
        }
      }

      var options = new Dictionary<string, string>();

      ReadNodesFromConfiguration(nodeList, options);

      ReadIntValue(options, "PingInterval", optionValue => ApplicationOptions.PingInterval = optionValue);
      ReadIntValue(options, "PingTimeout", optionValue => ApplicationOptions.PingTimeout = optionValue);
      ReadIntValue(options, "TTL", optionValue => ApplicationOptions.TTL = optionValue);
      ReadBoolValue(options, "DontFragment", optionValue => ApplicationOptions.DontFragment = optionValue);
      ReadBoolValue(options, "UseCustomBuffer", optionValue => ApplicationOptions.UseCustomBuffer = optionValue);
      ReadBytesValue(options, "Buffer", optionValue => ApplicationOptions.Buffer = optionValue);
      ReadIntValue(options, "AlertThreshold", optionValue => ApplicationOptions.AlertThreshold = optionValue);
      ReadBoolValue(options, "IsEmailAlertEnabled", optionValue => ApplicationOptions.IsEmailAlertEnabled = optionValue);
      ReadBoolValue(options, "IsEmailAuthenticationRequired", optionValue => ApplicationOptions.IsEmailAuthenticationRequired = optionValue);
      ReadStringValue(options, "EmailServer", optionValue => ApplicationOptions.EmailServer = optionValue);
      ReadStringValue(options, "EmailPort", optionValue => ApplicationOptions.EmailPort = optionValue);
      ReadStringValue(options, "EmailRecipient", optionValue => ApplicationOptions.EmailRecipient = optionValue);
      ReadStringValue(options, "EmailFromAddress", optionValue => ApplicationOptions.EmailFromAddress = optionValue);
      ReadBoolValue(options, "IsAudioAlertEnabled", optionValue => ApplicationOptions.IsAudioAlertEnabled = optionValue);
      ReadStringValue(options, "AudioFilePath", optionValue => ApplicationOptions.AudioFilePath = optionValue);
      ReadBoolValue(options, "IsLogOutputEnabled", optionValue => ApplicationOptions.IsLogOutputEnabled = optionValue);
      ReadStringValue(options, "LogPath", optionValue => ApplicationOptions.LogPath = optionValue);
      ReadBoolValue(options, "IsLogStatusChangesEnabled", optionValue => ApplicationOptions.IsLogStatusChangesEnabled = optionValue);
      ReadStringValue(options, "LogStatusChangesPath", optionValue => ApplicationOptions.LogStatusChangesPath = optionValue);
      ReadEncryptedValue(options, "EmailUser", optionValue => ApplicationOptions.EmailUser = optionValue);
      ReadEncryptedValue(options, "EmailPassword", optionValue => ApplicationOptions.EmailPassword = optionValue);
    }

    private static void ReadNodesFromConfiguration(IEnumerable nodeList, IDictionary<string, string> options)
    {
      foreach (XmlNode node in nodeList)
      {
        if (node.Attributes != null)
        {
          options.Add(node.Attributes["name"].Value, node.InnerText);
        }
      }
    }
  }
}
