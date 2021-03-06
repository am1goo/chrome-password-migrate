﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SQLite;
using System.IO;

public abstract class BaseBrowser : IBrowser
{
  public abstract string name { get; }
  public abstract EncryptionMode supportedEncryption { get; }
  public abstract DirectoryInfo applicationDataPath { get; }
  public abstract string loginsTable { get; }
  public abstract Type loginsType { get; }

  public abstract bool Scan(IList<ScanResult> results);
  public abstract IList<ILogins> Get(string sqliteDataSource);
  public abstract int Insert(string sqliteDataSource, IList<ILogins> logins, Action<int, int> onProgress);

  protected bool OnScan(IList<ScanResult> results, string relativeFolderPath, string stateFileName, string[] loginSubfolderPatterns, string loginFileName)
  {
    string mainFolder = AppArgs.GetArgString("-scan-main-folder", string.Empty);
    if (string.IsNullOrEmpty(mainFolder))
    {
      mainFolder = applicationDataPath.FullName;
    }
    else
    {
      mainFolder = Path.Combine(mainFolder, applicationDataPath.Name);
    }

    string rootFolder = Path.Combine(mainFolder, relativeFolderPath);
    return ScanHelper.Scan(rootFolder, stateFileName, loginSubfolderPatterns, loginFileName, results);
  }

  protected IList<ILogins> OnGet(string sqliteDataSource)
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

      DataTable table = SQLiteHelper.SelectAll(conn, loginsTable);
      return BrowserHelper.Parse(table, loginsType);
    }
  }

  protected int OnInsert(string sqliteDataSource, IList<ILogins> logins, Action<int, int> onProgress)
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