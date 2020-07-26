using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Data.SQLite;
using System.Data.Common;
using System.Data;
using System.Collections;

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

    List<ScanResult> results = new List<ScanResult>();

    Scan(results);

    if (results.Count == 0)
    {
      Console.ReadKey();
      return;
    }

    int srcIndex = SelectIndex("source index", results);
    int destIndex = SelectIndex("destination index", results);

    Copy(srcIndex, destIndex, results);

    Console.ReadKey();
  }

  private static void FindBrowsers(IList<IBrowser> list)
  {
    Type interfactType = typeof(IBrowser);
    foreach (Type type in Assembly.GetExecutingAssembly().GetTypes())
    {
      if (type.IsInterface) continue;
      if (type.IsAbstract) continue;

      if (interfactType.IsAssignableFrom(type))
      {
        IBrowser impl = Activator.CreateInstance(type) as IBrowser;
        list.Add(impl);
      }
    }
  }

  private static int SelectIndex(string name, IList<ScanResult> results)
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

  private static void Scan(IList<ScanResult> results)
  {
    for (int i = 0; i < browsers.Count; ++i)
    {
      IBrowser browser = browsers[i];
      List<string> paths = new List<string>();
      if (browser.Scan(paths))
      {
        foreach (string path in paths)
        {
          results.Add(new ScanResult(browser, path));
        }
      }
    }

    if (results.Count > 0)
    {
      for (int i = 0; i < results.Count; ++i)
      {
        ScanResult result = results[i];
        Console.WriteLine(string.Format("{0}. {1} ({2})", i, result.path, result.browser.name));
      }
    }
    else
    {
      Console.WriteLine("nothing found");
    }
  }

  private static void Copy(int srcIndex, int destIndex, IList<ScanResult> results)
  {
    if (srcIndex == destIndex)
    {
      Console.WriteLine("Can't copy to same destination");
      return;
    }

    if (!GetLogins(results[srcIndex], out IList<ILogins> srcLogins))
      return;

    if (!CopyLogins(results[destIndex], srcLogins, out int srcSkipped, out int srcCopied))
      return;

    Console.WriteLine(string.Format("Copied {0} of {1} logins. Skipped {2} logins", srcCopied, srcLogins.Count - srcSkipped, srcSkipped));
  }

  private static bool GetLogins(ScanResult scanResult, out IList<ILogins> results)
  {
    string path = scanResult.path;
    IBrowser browser = scanResult.browser;

    results = browser.Get(path);
    return results != null;
  }

  private static bool CopyLogins(ScanResult scanResult, IList<ILogins> srcLogins, out int skipped, out int copied)
  {
    string path = scanResult.path;
    IBrowser browser = scanResult.browser;

    IList<ILogins> logins = browser.Get(path);
    if (logins == null)
    {
      skipped = 0;
      copied = 0;
      return false;
    }

    IList<ILogins> loginsToAdd = new List<ILogins>();
    skipped = 0;
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
        ILogins destLogin = BrowserHelper.Copy<ILogins>(srcLogin, browser.loginsType);
        loginsToAdd.Add(destLogin);
      }
    }

    if (loginsToAdd.Count == 0)
    {
      copied = 0;
      return true;
    }

    copied = browser.Insert(path, loginsToAdd, (cur, total)=>
    {
      Console.WriteLine(string.Format("Copying {0} of {1}                                ", cur, total));
      Console.SetCursorPosition(Console.CursorLeft, Console.CursorTop - 1);
    });
    return true;
  }

  private static bool Validate(ConsoleKeyInfo keyInfo, IList<ScanResult> results, out int index)
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

  public struct ScanResult
  {
    public IBrowser browser;
    public string path;

    public ScanResult(IBrowser browser, string path)
    {
      this.browser = browser;
      this.path = path;
    }
  }
}