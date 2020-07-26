using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SQLite;
using System.IO;

public class YandexBrowser : IBrowser
{
  private const string RELATIVE_FOLDER_PATH = @"Yandex\YandexBrowser\User Data\";
  private const string LOGIN_FILE_NAME = "Ya Login Data";

  public string name { get { return "Yandex Browser"; } }
  public string loginsTable { get { return "logins"; } }
  public Type loginsType { get { return typeof(YandexBrowserLogins); } }

  public YandexBrowser() { }

  public bool Scan(IList<string> results)
  {
    string mainFolder = AppArgs.GetArgString("-scan-main-folder", Constants.APP_DATA_LOCAL.FullName);
    string rootFolder = Path.Combine(mainFolder, RELATIVE_FOLDER_PATH);
    return ScanHelper.Scan(rootFolder, LOGIN_FILE_NAME, results);
  }

  public IList<ILogins> Get(string sqliteDataSource)
  {
    SQLiteFactory factory = (SQLiteFactory)DbProviderFactories.GetFactory("System.Data.SQLite");
    using (SQLiteConnection conn = (SQLiteConnection)factory.CreateConnection())
    {
      conn.ConnectionString = "Data Source = " + sqliteDataSource;
      conn.Open();

      DataTable allTables = SQLiteHelper.GetAllTables(conn);
      if (!SQLiteHelper.CheckValueExists(allTables, "name", loginsTable))
      {
        Console.WriteLine(string.Format("Can't find table '{0}' in db at path {1}", loginsTable, sqliteDataSource));
        return null;
      }

      IList<ILogins> results = new List<ILogins>();
      DataTable table = SQLiteHelper.SelectAll(conn, loginsTable);
      IList<YandexBrowserLogins> logins = BrowserHelper.Parse<YandexBrowserLogins>(table);
      foreach (var v in logins)
      {
        results.Add(v);
      }
      return results;
    }
  }

  public int Insert(string sqliteDataSource, IList<ILogins> logins, Action<int, int> onProgress)
  {
    SQLiteFactory factory = (SQLiteFactory)DbProviderFactories.GetFactory("System.Data.SQLite");
    using (SQLiteConnection conn = (SQLiteConnection)factory.CreateConnection())
    {
      conn.ConnectionString = "Data Source = " + sqliteDataSource;
      conn.Open();

      DataTable allTables = SQLiteHelper.GetAllTables(conn);
      if (!SQLiteHelper.CheckValueExists(allTables, "name", loginsTable))
      {
        Console.WriteLine(string.Format("Can't find table '{0}' in db at path {1}", loginsTable, sqliteDataSource));
        return 0;
      }

      return SQLiteHelper.InsertAll(conn, loginsTable, BrowserHelper.Convert(logins), onProgress);
    }
  }
}