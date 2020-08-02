using System;
using System.Collections.Generic;
using System.Reflection;

public class Program
{
  private static readonly IList<IBrowser> browsers = new List<IBrowser>();

  private static bool log
  {
    get
    {
#if DEBUG
      return true;
#else
      return false;
#endif
    }
  }

  static void Main(string[] args)
  {
    AssemblyName assemblyName = Assembly.GetExecutingAssembly().GetName();
    Console.WriteLine(string.Format("Welcome to {0} ({1})", assemblyName.Name, assemblyName.Version));

    AppArgs.Parse(args, log);

    FindBrowsers(browsers);
    string supportedBrowsers = string.Empty;
    for (int i = 0; i < browsers.Count; ++i)
    {
      if (i > 0)
        supportedBrowsers += ", ";
      supportedBrowsers += browsers[i].name;
    }
    Console.WriteLine("Found supported browsers: {0}", supportedBrowsers);

    List<BrowserScanResult> results = new List<BrowserScanResult>();

    Scan(results);

    if (results.Count == 0)
    {
      Console.WriteLine("Press any key to exit...");
      Console.ReadKey();
      return;
    }

    int srcIndex = SelectIndex("source index", results);
    int destIndex = SelectIndex("destination index", results);

    Copy(srcIndex, destIndex, results);

    Console.WriteLine("Press any key to exit...");
    Console.ReadKey();
  }

  private static void FindBrowsers(IList<IBrowser> list)
  {
    List<IBrowser> results = new List<IBrowser>();
    Type interfactType = typeof(IBrowser);
    foreach (Type type in Assembly.GetExecutingAssembly().GetTypes())
    {
      if (type.IsInterface) continue;
      if (type.IsAbstract) continue;

      if (interfactType.IsAssignableFrom(type))
      {
        IBrowser impl = Activator.CreateInstance(type) as IBrowser;
        results.Add(impl);
      }
    }

    results.Sort(SortBrowsersByName);
    for (int i = 0; i < results.Count; ++i)
    {
      list.Add(results[i]);
    }
  }

  private static int SortBrowsersByName(IBrowser x, IBrowser y)
  {
    return x.GetType().Name.CompareTo(y.GetType().Name);
  }

  private static int SelectIndex(string name, IList<BrowserScanResult> results)
  {
    int idx = -1;
    while (idx < 0)
    {
      Console.Write("Select: {0} ", name);
      ConsoleKeyInfo idxKeyInfo = Console.ReadKey();
      if (Validate(idxKeyInfo, results, out int index))
      {
        idx = index;
        Console.Write(Environment.NewLine);
      }
      else
      {
        Console.SetCursorPosition(Console.CursorLeft, Console.CursorTop - 1);
        Console.WriteLine();
      }
    }
    return idx;
  }

  private static void Scan(IList<BrowserScanResult> results)
  {
    for (int i = 0; i < browsers.Count; ++i)
    {
      IBrowser browser = browsers[i];
      List<ScanResult> scans = new List<ScanResult>();
      if (browser.Scan(scans))
      {
        foreach (ScanResult scan in scans)
        {
          results.Add(new BrowserScanResult(browser, scan));
        }
      }
    }

    if (results.Count > 0)
    {
      for (int i = 0; i < results.Count; ++i)
      {
        BrowserScanResult result = results[i];
        Console.WriteLine(string.Format("{0}. {1} ({2})", i, result.scan.path, result.browser.name));
      }
    }
    else
    {
      Console.WriteLine("Nothing found");
    }
  }

  private static void Copy(int srcIndex, int destIndex, IList<BrowserScanResult> results)
  {
    if (srcIndex == destIndex)
    {
      Console.WriteLine("Can't copy to same destination");
      return;
    }

    BrowserScanResult srcScanResult = results[srcIndex];
    if (!GetLogins(srcScanResult, out IList<ILogins> srcLogins))
      return;

    BrowserScanResult descScanResult = results[destIndex];
    if (!CopyLogins(srcScanResult, descScanResult, srcLogins, out int srcSkipped, out int srcCopied, out int srcFailed))
      return;

    Console.WriteLine(string.Format("Copied {0} of {1} logins. Skipped {2} logins, failed {3} logins", srcCopied, srcLogins.Count - srcSkipped - srcFailed, srcSkipped, srcFailed));
  }

  private static bool GetLogins(BrowserScanResult scanResult, out IList<ILogins> results)
  {
    string path = scanResult.scan.path;
    IBrowser browser = scanResult.browser;

    results = browser.Get(path);
    return results != null;
  }

  private static bool CopyLogins(BrowserScanResult srcScanResult, BrowserScanResult destScanResult, IList<ILogins> srcLogins, out int skipped, out int copied, out int failed)
  {
    IBrowser srcBrowser = srcScanResult.browser;
    string srcEncryptedKey = srcScanResult.scan.encryptedKey;

    string destPath = destScanResult.scan.path;
    IBrowser destBrowser = destScanResult.browser;
    string destEncrpytedKey = destScanResult.scan.encryptedKey;

    IList<ILogins> logins = destBrowser.Get(destPath);
    if (logins == null)
    {
      skipped = 0;
      copied = 0;
      failed = 0;
      return false;
    }

    IList<ILogins> loginsToAdd = new List<ILogins>();
    skipped = 0;
    failed = 0;

    for (int i = 0; i < srcLogins.Count; ++i)
    {
      ILogins srcLogin = srcLogins[i];
      if (CheckLoginsExists(logins, srcLogin))
      {
        skipped++;
        continue;
      }
      else
      {
        ILogins destLogin = BrowserHelper.Copy<ILogins>(srcLogin, srcBrowser, srcEncryptedKey, destBrowser, destEncrpytedKey);
        if (destLogin == null)
        {
          failed++;
          continue;
        }
        loginsToAdd.Add(destLogin);
      }
    }

    if (loginsToAdd.Count == 0)
    {
      copied = 0;
      return true;
    }

    copied = destBrowser.Insert(destPath, loginsToAdd, (cur, total)=>
    {
      Console.WriteLine(string.Format("Copying {0} of {1}                                ", cur, total));
      Console.SetCursorPosition(Console.CursorLeft, Console.CursorTop - 1);
    });
    return true;
  }

  private static bool Validate(ConsoleKeyInfo keyInfo, IList<BrowserScanResult> results, out int index)
  {
    string str = new string(keyInfo.KeyChar, 1);
    if (!int.TryParse(str, out index))
      return false;

    if (index < 0 || index >= results.Count)
      return false;

    return true;
  }

  private static bool CheckLoginsExists(IList<ILogins> list, ILogins value)
  {
    foreach (var v in list)
    {
      if (IsSimilar(v, value))
        return true;
    }
    return false;
  }

  private static bool IsSimilar(ILogins one, ILogins two)
  {
    return
      one.OriginUrl == two.OriginUrl &&
      one.UsernameElement == two.UsernameElement &&
      one.UsernameValue == two.UsernameValue &&
      one.PasswordElement == two.PasswordElement &&
      one.PasswordType == two.PasswordType &&
      one.SignonRealm == two.SignonRealm;
  }
}