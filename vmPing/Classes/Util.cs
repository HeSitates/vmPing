using System;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using vmPing.Properties;

namespace vmPing.Classes
{
  public class Util
  {
    public static void ShowError(string message)
    {
      MessageBox.Show(message, Strings.Error_WindowTitle, MessageBoxButton.OK, MessageBoxImage.Error);
    }

    public static bool IsValidHtmlColor(string htmlColor)
    {
      var regex = new Regex("^#([0-9A-Fa-f]{3}|[0-9A-Fa-f]{6}|[0-9A-Fa-f]{8})$");

      return regex.IsMatch(htmlColor);
    }

    public static string GetSafeFilename(string filename)
    {
      // Manually defining invalid characters rather than using Path.GetInvalidFileNameChars(),
      // as that method seems to be missing several invalid filename characters.
      char[] invalidCharacters = { '<', '>', ':', '"', '/', '\\', '|', '?', '*' };
      return string.Join("_", filename.Split(invalidCharacters));
    }
  }
}
