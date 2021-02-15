namespace vmPing.Classes
{
  public interface IEncryptor
  {
    string EncryptString(string plainText);
    string DecryptString(string cipherText);
  }
}