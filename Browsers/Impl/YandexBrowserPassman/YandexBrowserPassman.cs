﻿using System;
using System.Collections.Generic;

public class YandexBrowserPassman : BaseBrowser
{
  private const string RELATIVE_FOLDER_PATH = @"Yandex\YandexBrowser\User Data\";
  private const string LOGIN_FILE_NAME = "Ya Passman Data";

  public override string name { get { return "Yandex Browser (Passman)"; } }
  public override string loginsTable { get { return "logins"; } }
  public override Type loginsType { get { return typeof(YandexBrowserPassmanLogins); } }

  public YandexBrowserPassman() { }

  public override bool Scan(IList<string> results)
  {
    bool res = false;
    res |= OnScan(results, RELATIVE_FOLDER_PATH, LOGIN_FILE_NAME);
    return true;
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