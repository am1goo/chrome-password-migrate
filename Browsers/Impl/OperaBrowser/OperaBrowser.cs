using System;
using System.Collections.Generic;
using System.IO;

public class OperaBrowser : BaseBrowser
{
  private const string RELATIVE_FOLDER_PATH = @"Opera Software\Opera Stable\";
  private const string STATE_FILE_NAME = "Local State";

  private static readonly string[] LOGIN_SUBFOLDER_PATTERNS = new string[0];
  private const string LOGIN_FILE_NAME = "Login Data";

  public override string name { get { return "Opera Browser"; } }
  public override DirectoryInfo applicationDataPath { get { return BrowserHelper.Directory(Environment.SpecialFolder.ApplicationData); } }
  public override string loginsTable { get { return "logins"; } }
  public override Type loginsType { get { return typeof(OperaBrowserLogins); } }

  public OperaBrowser() { }

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
}