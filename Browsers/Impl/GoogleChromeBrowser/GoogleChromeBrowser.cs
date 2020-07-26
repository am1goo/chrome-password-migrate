using System;
using System.Collections.Generic;

public class GoogleChromeBrowser : BaseBrowser
{
  private const string RELATIVE_FOLDER_PATH = @"Google\Chrome\User Data\";
  private const string LOGIN_FILE_NAME = "Login Data";

  public override string name { get { return "Chrome Browser"; } }
  public override string loginsTable { get { return "logins"; } }
  public override Type loginsType { get { return typeof(GoogleChromeBrowserLogins); } }

  public GoogleChromeBrowser() { }

  public override bool Scan(IList<string> results)
  {
    return OnScan(results, RELATIVE_FOLDER_PATH, LOGIN_FILE_NAME);
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