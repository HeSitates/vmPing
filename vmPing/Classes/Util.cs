﻿using System;
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
    public static void SendEmail(string hostStatus, string hostName)
    {
      var serverAddress    = ApplicationOptions.EmailServer;
      var serverUser       = ApplicationOptions.EmailUser;
      var serverPassword   = ApplicationOptions.EmailPassword;
      var serverPort       = ApplicationOptions.EmailPort;
      var mailFromAddress  = ApplicationOptions.EmailFromAddress;
      var mailFromFriendly = "vmPing";
      var mailToAddress    = ApplicationOptions.EmailRecipient;
      var mailSubject      = $"[vmPing] {hostName} <> {Strings.Email_Host} {hostStatus}";
      var mailBody         = $"{hostName} {Strings.Email_Verb} {hostStatus}.{Environment.NewLine}{DateTime.Now.ToLongDateString()}  {DateTime.Now.ToLongTimeString()}";

      var message = new MailMessage();

      try
      {
        var smtpClient = new SmtpClient();
        var fromAddress = mailFromFriendly.Length > 0 ? new MailAddress(mailFromAddress, mailFromFriendly) : new MailAddress(mailFromAddress);

        smtpClient.Host = serverAddress;

        if (ApplicationOptions.IsEmailAuthenticationRequired)
        {
          smtpClient.Credentials = new NetworkCredential(serverUser, serverPassword);
        }

        if (serverPort.Length > 0)
        {
          smtpClient.Port = int.Parse(serverPort);
        }

        message.From = fromAddress;
        message.Subject = mailSubject;
        message.Body = mailBody;

        message.To.Add(mailToAddress);

        //Send the email.
        smtpClient.Send(message);
      }
      catch
      {
        // There was an error sending Email.
      }
      finally
      {
        message.Dispose();
      }
    }

    public static void SendTestEmail(string serverAddress, string serverPort, bool isAuthRequired, string username, System.Security.SecureString password, string mailFrom, string mailRecipient)
    {
      const string mailFromFriendly = "vmPing";
      const string mailSubject      = "[vmPing] Test Email Notification";
      var          mailBody         = $"{DateTime.Now.ToLongDateString()} {DateTime.Now.ToLongTimeString()} - This is a test email notification sent by vmPing.";
      
      using (var smtpClient = new SmtpClient())
      {
        smtpClient.Host = serverAddress;

        if (serverPort.Length > 0)
        {
          smtpClient.Port = int.Parse(serverPort);
        }

        var fromAddress = new MailAddress(mailFrom, mailFromFriendly);

        if (isAuthRequired)
        {
          smtpClient.Credentials = new NetworkCredential(username, password);
        }

        using (var message = new MailMessage())
        {
          message.From = fromAddress;
          message.Subject = mailSubject;
          message.Body = mailBody;
          message.To.Add(mailRecipient);

          //Send the email.
          smtpClient.Send(message);
        }
      }
    }

    public static void ShowError(string message)
    {
      MessageBox.Show(message, Strings.Error_WindowTitle, MessageBoxButton.OK, MessageBoxImage.Error);
    }

    public static bool IsValidHtmlColor(string htmlColor)
    {
      var regex = new Regex("^#([0-9A-Fa-f]{3}|[0-9A-Fa-f]{6}|[0-9A-Fa-f]{8})$");

      return regex.IsMatch(htmlColor);
    }

    // ReSharper disable once InconsistentNaming
    public static string EncryptStringAES(string plainText)
    {
      if (string.IsNullOrEmpty(plainText))
      {
        throw new ArgumentNullException(nameof(plainText));
      }

      RijndaelManaged aesAlgorithm = null;                 // RijndaelManaged object used to encrypt the data.

      try
      {
        // Generate the key from a shared secret and initilization vector.
        var key = new Rfc2898DeriveBytes("https://github.com/R-Smith/vmPing" + Environment.MachineName, Encoding.ASCII.GetBytes(Environment.UserName + "@@vmping-salt@@"));

        // Create a RijndaelManaged object.
        aesAlgorithm     = new RijndaelManaged { Padding = PaddingMode.PKCS7 };
        aesAlgorithm.Key = key.GetBytes(aesAlgorithm.KeySize / 8);

        key.Dispose();

        // Create a decryptor to perform the stream transform.
        var encryptor = aesAlgorithm.CreateEncryptor(aesAlgorithm.Key, aesAlgorithm.IV);

        // Create the streams used for encryption.
        using (var memoryStream = new MemoryStream())
        {
          // Prepend the IV.
          memoryStream.Write(BitConverter.GetBytes(aesAlgorithm.IV.Length), 0, sizeof(int));
          memoryStream.Write(aesAlgorithm.IV, 0, aesAlgorithm.IV.Length);
          using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
          using (var streamWriter = new StreamWriter(cryptoStream))
          {
            // Write all data to the stream.
            streamWriter.Write(plainText);
          }

          // Return the encrypted bytes from the memory stream.
          return Convert.ToBase64String(memoryStream.ToArray());
        }
      }
      finally
      {
        // Clear the RijndaelManaged object.
        aesAlgorithm?.Clear();
        aesAlgorithm?.Dispose();
      }
    }

    public static string GetSafeFilename(string filename)
    {
      // Manually defining invalid characters rather than using Path.GetInvalidFileNameChars(),
      // as that method seems to be missing several invalid filename characters.
      char[] invalidCharacters = { '<', '>', ':', '"', '/', '\\', '|', '?', '*' };
      return string.Join("_", filename.Split(invalidCharacters));
    }

    // ReSharper disable once InconsistentNaming
    public static string DecryptStringAES(string cipherText)
    {
      if (string.IsNullOrEmpty(cipherText))
      {
        throw new ArgumentNullException(nameof(cipherText));
      }

      // Declare the RijndaelManaged object used to decrypt the data.
      RijndaelManaged aesAlgorithm = null;

      try
      {
        // Generate the key from a shared secret and initilization vector.
        var key = new Rfc2898DeriveBytes("https://github.com/R-Smith/vmPing" + Environment.MachineName, Encoding.ASCII.GetBytes(Environment.UserName + "@@vmping-salt@@"));

        // Create the streams used for decryption.                
        var bytes = Convert.FromBase64String(cipherText);
        using (var memoryStream = new MemoryStream(bytes))
        {
          // Create a RijndaelManaged object with the specified key and IV.
          aesAlgorithm     = new RijndaelManaged { Padding = PaddingMode.PKCS7 };
          aesAlgorithm.Key = key.GetBytes(aesAlgorithm.KeySize / 8);
          
          // Get the initialization vector from the encrypted stream.
          aesAlgorithm.IV = ReadByteArray(memoryStream);
          
          // Create a decrytor to perform the stream transform.
          var decryptor = aesAlgorithm.CreateDecryptor(aesAlgorithm.Key, aesAlgorithm.IV);
          using (var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
          using (var streamReader = new StreamReader(cryptoStream))
          {
            // Read the decrypted bytes from the decrypting stream and place them in a string.
            var plaintext = streamReader.ReadToEnd();
            return plaintext;
          }
        }
      }
      finally
      {
        // Clear the RijndaelManaged object.
        aesAlgorithm?.Clear();
      }
    }

    private static byte[] ReadByteArray(Stream stream)
    {
      var rawLength = new byte[sizeof(int)];
      if (stream.Read(rawLength, 0, rawLength.Length) != rawLength.Length)
      {
        throw new SystemException("Stream did not contain properly formatted byte array");
      }

      var buffer = new byte[BitConverter.ToInt32(rawLength, 0)];
      if (stream.Read(buffer, 0, buffer.Length) != buffer.Length)
      {
        throw new SystemException("Did not read byte array properly");
      }

      return buffer;
    }
  }
}
