namespace vmPing.Classes
{
  public interface IEmailSender
  {
    void SendEmail(string     hostStatus,    string hostName);
    void SendTestEmail(string serverAddress, string serverPort, bool isAuthRequired, string username, System.Security.SecureString password, string mailFrom, string mailRecipient);
  }
}