using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Parameters;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

public static class EncryptHelper
{
  public class AesGcm256
  {
    public static bool Decrypt(byte[] password, string encryptedKey, out string pwd)
    {
      if (!TryParse(encryptedKey, out byte[] encryptedBytes))
      {
        pwd = null;
        return false;
      }

      Prepare(encryptedBytes, out byte[] version, out byte[] nonce, out byte[] ciphertextTag);

      return Decrypt(password, encryptedBytes, nonce, ciphertextTag, out pwd);
    }

    private static bool Decrypt(byte[] password, byte[] encryptedKey, byte[] nonce, byte[] ciphertextTag, out string pwd)
    {
      try
      {
        AeadParameters parameters = new AeadParameters(new KeyParameter(encryptedKey), 128, nonce, null);

        GcmBlockCipher cipher = new GcmBlockCipher(new AesEngine());
        cipher.Init(false, parameters);

        int plainBytesLength = cipher.GetOutputSize(password.Length);
        byte[] plainBytes = new byte[plainBytesLength];

        int retLen = cipher.ProcessBytes(password, 0, password.Length, plainBytes, 0);
        cipher.DoFinal(plainBytes, retLen);

        pwd = Encoding.UTF8.GetString(plainBytes).TrimEnd("\r\n\0".ToCharArray());
        return true;
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex);

        pwd = null;
        return false;
      }
    }

    public static bool TryParse(string key, out byte[] encryptedKey)
    {
      try
      {
        encryptedKey = Parse(key);
        return true;
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex);

        encryptedKey = null;
        return false;
      }
    }

    public static byte[] Parse(string key)
    {
      byte[] src = Convert.FromBase64String(key);
      byte[] protectedKey = src.Skip(5).ToArray();

      return ProtectedData.Unprotect(protectedKey, null, DataProtectionScope.CurrentUser);
    }

    private static void Prepare(byte[] encryptedBytes, out byte[] version, out byte[] nonce, out byte[] ciphertextTag)
    {
      version = new byte[3];
      nonce = new byte[12];
      ciphertextTag = new byte[encryptedBytes.Length - version.Length - nonce.Length];

      Array.Copy(encryptedBytes, 0, version, 0, version.Length);
      Array.Copy(encryptedBytes, version.Length, nonce, 0, nonce.Length);
      Array.Copy(encryptedBytes, version.Length + nonce.Length, ciphertextTag, 0, ciphertextTag.Length);
    }
  }
}