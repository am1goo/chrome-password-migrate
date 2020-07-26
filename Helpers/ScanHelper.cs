using System.Collections.Generic;
using System.IO;

public static class ScanHelper
{
  public static bool Scan(string rootPath, string fileName, IList<string> results)
  {
    DirectoryInfo di = new DirectoryInfo(rootPath);
    return Scan(di, fileName, results);
  }

  public static bool Scan(DirectoryInfo rootPath, string fileName, IList<string> results)
  { 
    if (!rootPath.Exists)
    {
      results = null;
      return false;
    }

    bool scan = false;

    scan |= Scan(rootPath, "Default", fileName, results);
    scan |= Scan(rootPath, "Profile*", fileName, results);

    return scan;
  }

  public static bool Scan(DirectoryInfo rootPath, string subfolderPattern, string fileName, IList<string> results)
  {
    DirectoryInfo[] dis = rootPath.GetDirectories(subfolderPattern, SearchOption.TopDirectoryOnly);
    foreach (var di in dis)
    {
      foreach (var fi in di.GetFiles())
      {
        if (fi.Name == fileName)
        {
          results.Add(fi.FullName);
          return true;
        }
      }
    }
    return false;
  }
}