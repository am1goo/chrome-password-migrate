/*using System;
using System.Collections.Generic;
using System.IO;

public class YandexBrowser : BaseBrowser
{
  private const string RELATIVE_FOLDER_PATH = @"Yandex\YandexBrowser\User Data\";
  private const string STATE_FILE_NAME = "Local State";

  private static readonly string[] LOGIN_SUBFOLDER_PATTERNS = new string[] { "Default", "Profile*" };
  private const string LOGIN_FILE_NAME = "Ya Login Data";

  public override string name { get { return "Yandex Browser"; } }
  public override DirectoryInfo applicationDataPath { get { return BrowserHelper.Directory(Environment.SpecialFolder.LocalApplicationData); } }
  public override string loginsTable { get { return "logins"; } }
  public override Type loginsType { get { return typeof(YandexBrowserLogins); } }

  public YandexBrowser() { }

  public override bool Scan(IList<ScanResult> results)
  {
    return OnScan(results, RELATIVE_FOLDER_PATH, STATE_FILE_NAME, LOGIN_SUBFOLDER_PATTERNS, LOGIN_FILE_NAME);
  }

  public override IList<ILogins> Get(string sqliteDataSource)
  {
    return OnGet(sqliteDataSource);
  }

  public override int Insert(string sqliteDataSource, IList<ILogins> logins, Action<int, int> onProgress)
  {
    return OnInsert(sqliteDataSource, logins, onProgress);
  }
}*/