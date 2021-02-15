namespace vmPing.Classes
{
  public class EmailSender : IEmailSender
  {
    public void SendEmail(string hostStatus, string hostName)
    {
      var serverAddress    = ApplicationOptions.EmailServer;
      var serverUser       = ApplicationOptions.EmailUser;
      var serverPassword   = ApplicationOptions.EmailPassword;
      var serverPort       = ApplicationOptions.EmailPort;
      var mailFromAddress  = ApplicationOptions.EmailFromAddress;
      var mailFromFriendly = "vmPing";
      var mailToAddress    = ApplicationOptions.EmailRecipient;
      var mailSubject      = $"[vmPing] {hostName} <> {vmPing.Properties.Strings.Email_Host} {hostStatus}";
      var mailBody         = $"{hostName} {vmPing.Properties.Strings.Email_Verb} {hostStatus}.{System.Environment.NewLine}{System.DateTime.Now.ToLongDateString()}  {System.DateTime.Now.ToLongTimeString()}";

      var message = new System.Net.Mail.MailMessage();

      try
      {
        var smtpClient  = new System.Net.Mail.SmtpClient();
        var fromAddress = mailFromFriendly.Length > 0 ? new System.Net.Mail.MailAddress(mailFromAddress, mailFromFriendly) : new System.Net.Mail.MailAddress(mailFromAddress);

        smtpClient.Host = serverAddress;

        if (ApplicationOptions.IsEmailAuthenticationRequired)
        {
          smtpClient.Credentials = new System.Net.NetworkCredential(serverUser, serverPassword);
        }

        if (serverPort.Length > 0)
        {
          smtpClient.Port = int.Parse(serverPort);
        }

        message.From    = fromAddress;
        message.Subject = mailSubject;
        message.Body    = mailBody;

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

    public void SendTestEmail(string serverAddress, string serverPort, bool isAuthRequired, string username, System.Security.SecureString password, string mailFrom, string mailRecipient)
    {
      const string mailFromFriendly = "vmPing";
      const string mailSubject      = "[vmPing] Test Email Notification";
      var          mailBody         = $"{System.DateTime.Now.ToLongDateString()} {System.DateTime.Now.ToLongTimeString()} - This is a test email notification sent by vmPing.";

      using (var smtpClient = new System.Net.Mail.SmtpClient())
      {
        smtpClient.Host = serverAddress;

        if (serverPort.Length > 0)
        {
          smtpClient.Port = int.Parse(serverPort);
        }

        var fromAddress = new System.Net.Mail.MailAddress(mailFrom, mailFromFriendly);

        if (isAuthRequired)
        {
          smtpClient.Credentials = new System.Net.NetworkCredential(username, password);
        }

        using (var message = new System.Net.Mail.MailMessage())
        {
          message.From    = fromAddress;
          message.Subject = mailSubject;
          message.Body    = mailBody;
          message.To.Add(mailRecipient);

          //Send the email.
          smtpClient.Send(message);
        }
      }
    }
  }
}