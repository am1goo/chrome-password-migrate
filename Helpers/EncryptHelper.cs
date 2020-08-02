using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Parameters;
using System;
using System.Security.Cryptography;
using System.Text;

public static class EncryptHelper
{
  public enum Result
  {
    Success = 0,
    DpapiInvalidData = 1,
    AesGcm256Failed = 2,
    UnsupportedProtocol = 3,
    InvalidEncryptedKey = 4,
  }

  public static Result Encrypt(string pwd, string encryptedKey, out byte[] password)
  {
    Result res = ExtractEncryptedKey(encryptedKey, out byte[] encryptedBytes);
    if (res != Result.Success)
    {
      password = null;
      return res;
    }

    byte[] version = Encoding.ASCII.GetBytes("v10");
    string ver = Encoding.ASCII.GetString(version);
    switch (ver)
    {
      case "v10":
        byte[] nonce = AesGcm256.Nonce(12);
        Result v10res = AesGcm256.Encrypt(pwd, encryptedBytes, nonce, out byte[] ciphertextTag);
        if (v10res != Result.Success)
        {
          password = null;
          return v10res;
        }

        v10res = CombineCipherText(version, nonce, ciphertextTag, out password);
        return v10res;

      default:
        password = null;
        return Result.UnsupportedProtocol;
    }
  }

  public static Result Decrypt(byte[] password, string encryptedKey, out string pwd)
  {
    Result res = ExtractEncryptedKey(encryptedKey, out byte[] encryptedBytes);
    if (res != Result.Success)
    {
      pwd = null;
      return res;
    }

    res = ExtractCipherText(password, out byte[] version, out byte[] nonce, out byte[] ciphertextTag);
    if (res != Result.Success)
    {
      pwd = null;
      return res;
    }

    string ver = Encoding.ASCII.GetString(version);
    switch (ver)
    {
      case "v10":
        return AesGcm256.Decrypt(encryptedBytes, nonce, ciphertextTag, out pwd);

      default:
        pwd = null;
        return Result.UnsupportedProtocol;
    }
  }

  public static Result ExtractEncryptedKey(string key, out byte[] encryptedKey)
  {
    byte[] src = Convert.FromBase64String(key);

    byte[] dpapiKey = new byte[5];
    byte[] protectedKey = new byte[src.Length - dpapiKey.Length];

    Array.Copy(src, 0, dpapiKey, 0, dpapiKey.Length);
    Array.Copy(src, dpapiKey.Length, protectedKey, 0, protectedKey.Length);

    return Dpapi.Decrypt(protectedKey, out encryptedKey);
  }

  public static Result CombineCipherText(byte[] version, byte[] nonce, byte[] ciphertextTag, out byte[] encryptedBytes)
  {
    encryptedBytes = new byte[version.Length + nonce.Length + ciphertextTag.Length];

    Array.Copy(version, 0, encryptedBytes, 0, version.Length);
    Array.Copy(nonce, 0, encryptedBytes, 0 + version.Length, nonce.Length);
    Array.Copy(ciphertextTag, 0, encryptedBytes, 0 + version.Length + nonce.Length, ciphertextTag.Length);
    return Result.Success;
  }

  public static Result ExtractCipherText(byte[] encryptedBytes, out byte[] version, out byte[] nonce, out byte[] ciphertextTag)
  {
    try
    {
      version = new byte[3];
      nonce = new byte[12];
      ciphertextTag = new byte[encryptedBytes.Length - version.Length - nonce.Length];

      Array.Copy(encryptedBytes, 0, version, 0, version.Length);
      Array.Copy(encryptedBytes, 0 + version.Length, nonce, 0, nonce.Length);
      Array.Copy(encryptedBytes, 0 + version.Length + nonce.Length, ciphertextTag, 0, ciphertextTag.Length);
      return Result.Success;
    }
    catch
    {
      version = null;
      nonce = null;
      ciphertextTag = null;
      return Result.InvalidEncryptedKey;
    }
  }

  public class Dpapi
  {
    public static Result Encrypt(byte[] unprotectedKey, out byte[] protectedKey)
    {
      protectedKey = ProtectedData.Protect(unprotectedKey, null, DataProtectionScope.CurrentUser);
      return Result.Success;
    }

    public static Result Decrypt(byte[] protectedKey, out byte[] unprotectedKey)
    {
      try
      {
        unprotectedKey = ProtectedData.Unprotect(protectedKey, null, DataProtectionScope.CurrentUser);
        return Result.Success;
      }
      catch
      {
        unprotectedKey = null;
        return Result.DpapiInvalidData;
      }
    }
  }

  public class AesGcm256
  {
    public static Encoding DEFAULT_ENCODING = Encoding.UTF8;

    public static byte[] Nonce(int size)
    {
      Random rnd = new Random();

      var iv = new byte[size];
      rnd.NextBytes(iv);
      return iv;
    }

    public static Result Encrypt(string pwd, byte[] encryptedKey, byte[] nonce, out byte[] ciphertextTag)
    {
      try
      {
        AeadParameters parameters = new AeadParameters(new KeyParameter(encryptedKey), 128, nonce, null);

        GcmBlockCipher cipher = new GcmBlockCipher(new AesEngine());
        cipher.Init(true, parameters);

        byte[] pwdBytes = DEFAULT_ENCODING.GetBytes(pwd);

        int encryptedBytesLength = cipher.GetOutputSize(pwdBytes.Length);
        byte[] encryptedBytes = new byte[encryptedBytesLength];

        int retLen = cipher.ProcessBytes(pwdBytes, 0, pwdBytes.Length, encryptedBytes, 0);
        cipher.DoFinal(encryptedBytes, retLen);

        ciphertextTag = encryptedBytes;
        return Result.Success;
      }
      catch
      {
        ciphertextTag = null;
        return Result.AesGcm256Failed;
      }
    }

    public static Result Decrypt(byte[] encryptedKey, byte[] nonce, byte[] ciphertextTag, out string pwd)
    {
      try
      {
        AeadParameters parameters = new AeadParameters(new KeyParameter(encryptedKey), 128, nonce, null);

        GcmBlockCipher cipher = new GcmBlockCipher(new AesEngine());
        cipher.Init(false, parameters);

        int pwdBytesLength = cipher.GetOutputSize(ciphertextTag.Length);
        byte[] pwdBytes = new byte[pwdBytesLength];

        int retLen = cipher.ProcessBytes(ciphertextTag, 0, ciphertextTag.Length, pwdBytes, 0);
        cipher.DoFinal(pwdBytes, retLen);

        pwd = DEFAULT_ENCODING.GetString(pwdBytes).TrimEnd("\r\n\0".ToCharArray());
        return Result.Success;
      }
      catch
      {
        pwd = null;
        return Result.AesGcm256Failed;
      }
    }
  }
}