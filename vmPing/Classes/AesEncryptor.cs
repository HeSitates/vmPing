namespace vmPing.Classes
{
  public class AesEncryptor : IEncryptor
  {
    public string EncryptString(string plainText)
    {
      if (string.IsNullOrEmpty(plainText))
      {
        throw new System.ArgumentNullException(nameof(plainText));
      }

      System.Security.Cryptography.RijndaelManaged aesAlgorithm = null; // RijndaelManaged object used to encrypt the data.

      try
      {
        // Generate the key from a shared secret and initilization vector.
        var key = new System.Security.Cryptography.Rfc2898DeriveBytes("https://github.com/R-Smith/vmPing" + System.Environment.MachineName, System.Text.Encoding.ASCII.GetBytes(System.Environment.UserName + "@@vmping-salt@@"));

        // Create a RijndaelManaged object.
        aesAlgorithm     = new System.Security.Cryptography.RijndaelManaged { Padding = System.Security.Cryptography.PaddingMode.PKCS7 };
        aesAlgorithm.Key = key.GetBytes(aesAlgorithm.KeySize / 8);

        key.Dispose();

        // Create a decryptor to perform the stream transform.
        var encryptor = aesAlgorithm.CreateEncryptor(aesAlgorithm.Key, aesAlgorithm.IV);

        // Create the streams used for encryption.
        using (var memoryStream = new System.IO.MemoryStream())
        {
          // Prepend the IV.
          memoryStream.Write(System.BitConverter.GetBytes(aesAlgorithm.IV.Length), 0, sizeof(int));
          memoryStream.Write(aesAlgorithm.IV,                               0, aesAlgorithm.IV.Length);
          using (var cryptoStream = new System.Security.Cryptography.CryptoStream(memoryStream, encryptor, System.Security.Cryptography.CryptoStreamMode.Write))
          using (var streamWriter = new System.IO.StreamWriter(cryptoStream))
          {
            // Write all data to the stream.
            streamWriter.Write(plainText);
          }

          // Return the encrypted bytes from the memory stream.
          return System.Convert.ToBase64String(memoryStream.ToArray());
        }
      }
      finally
      {
        // Clear the RijndaelManaged object.
        aesAlgorithm?.Clear();
        aesAlgorithm?.Dispose();
      }
    }

    public string DecryptString(string cipherText)
    {
      if (string.IsNullOrEmpty(cipherText))
      {
        throw new System.ArgumentNullException(nameof(cipherText));
      }

      // Declare the RijndaelManaged object used to decrypt the data.
      System.Security.Cryptography.RijndaelManaged aesAlgorithm = null;

      try
      {
        // Generate the key from a shared secret and initilization vector.
        var key = new System.Security.Cryptography.Rfc2898DeriveBytes("https://github.com/R-Smith/vmPing" + System.Environment.MachineName, System.Text.Encoding.ASCII.GetBytes(System.Environment.UserName + "@@vmping-salt@@"));

        // Create the streams used for decryption.                
        var bytes = System.Convert.FromBase64String(cipherText);
        using (var memoryStream = new System.IO.MemoryStream(bytes))
        {
          // Create a RijndaelManaged object with the specified key and IV.
          aesAlgorithm     = new System.Security.Cryptography.RijndaelManaged { Padding = System.Security.Cryptography.PaddingMode.PKCS7 };
          aesAlgorithm.Key = key.GetBytes(aesAlgorithm.KeySize / 8);

          // Get the initialization vector from the encrypted stream.
          aesAlgorithm.IV = ReadByteArray(memoryStream);

          // Create a decrytor to perform the stream transform.
          var decryptor = aesAlgorithm.CreateDecryptor(aesAlgorithm.Key, aesAlgorithm.IV);
          using (var cryptoStream = new System.Security.Cryptography.CryptoStream(memoryStream, decryptor, System.Security.Cryptography.CryptoStreamMode.Read))
          using (var streamReader = new System.IO.StreamReader(cryptoStream))
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

    private byte[] ReadByteArray(System.IO.Stream stream)
    {
      var rawLength = new byte[sizeof(int)];
      if (stream.Read(rawLength, 0, rawLength.Length) != rawLength.Length)
      {
        throw new System.SystemException("Stream did not contain properly formatted byte array");
      }

      var buffer = new byte[System.BitConverter.ToInt32(rawLength, 0)];
      if (stream.Read(buffer, 0, buffer.Length) != buffer.Length)
      {
        throw new System.SystemException("Did not read byte array properly");
      }

      return buffer;
    }
  }
}