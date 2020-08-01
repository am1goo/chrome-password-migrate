using System.Collections.Generic;
using System.IO;
using System.Json;

public static class ScanHelper
{
  public static bool Scan(string rootPath, string stateFileName, string[] loginSubfolderPatterns, string loginFileName, IList<ScanResult> results)
  {
    DirectoryInfo di = new DirectoryInfo(rootPath);
    return Scan(di, stateFileName, loginSubfolderPatterns, loginFileName, results);
  }

  public static bool Scan(DirectoryInfo rootPath, string stateFileName, string[] loginSubfolderPatterns, string loginFileName, IList<ScanResult> results)
  { 
    if (!rootPath.Exists)
      return false;

    bool resState = ScanState(rootPath, stateFileName, out string encryptedKey);
    if (!resState)
      return false;

    bool resLogins = ScanLogins(rootPath, loginSubfolderPatterns, loginFileName, encryptedKey, results);
    if (!resLogins)
      return false;

    return true;
  }

  public static bool ScanState(DirectoryInfo rootPath, string stateFileName, out string encryptKey)
  {
    FileInfo fi = new FileInfo(Path.Combine(rootPath.FullName, stateFileName));
    if (!fi.Exists)
    {
      encryptKey = null;
      return false;
    }

    using (StreamReader sr = fi.OpenText())
    {
      string json = sr.ReadToEnd();
      JsonValue state = JsonValue.Parse(json);
      if (!state.ContainsKey("os_crypt"))
      {
        encryptKey = null;
        return false;
      }

      JsonValue osCrypt = state["os_crypt"];
      if (!osCrypt.ContainsKey("encrypted_key"))
      {
        encryptKey = null;
        return false;
      }

      encryptKey = osCrypt["encrypted_key"];
      return true;
    }
  }

  public static bool ScanLogins(DirectoryInfo rootPath, string[] subfolderPatterns, string loginFileName, string encryptKey, IList<ScanResult> results)
  {
    if (subfolderPatterns != null && subfolderPatterns.Length > 0)
    {
      bool res = false;
      for (int i = 0; i < subfolderPatterns.Length; ++i)
      {
        res |= ScanLogin(rootPath, subfolderPatterns[i], loginFileName, encryptKey, results);
      }
      return res;
    }
    else
    {
      return ScanLogin(rootPath, loginFileName, encryptKey, results);
    }
  }

  public static bool ScanLogin(DirectoryInfo rootPath, string subfolderPattern, string loginFileName, string encryptKey, IList<ScanResult> results)
  {
    DirectoryInfo[] dis = rootPath.GetDirectories(subfolderPattern, SearchOption.TopDirectoryOnly);
    foreach (var di in dis)
    {
      bool res = ScanLogin(di, loginFileName, encryptKey, results);
      if (res)
        return true;
    }
    return false;
  }

  public static bool ScanLogin(DirectoryInfo rootPath, string loginFileName, string encryptKey, IList<ScanResult> results)
  {
    foreach (var fi in rootPath.GetFiles())
    {
      if (fi.Name == loginFileName)
      {
        results.Add(new ScanResult(fi.FullName, encryptKey));
        return true;
      }
    }
    return false;
  }
}