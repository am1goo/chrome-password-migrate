using System;
using System.Collections.Generic;
using System.IO;

public interface IBrowser
{
  string name { get; }
  DirectoryInfo applicationDataPath { get; }
  string loginsTable { get; }
  Type loginsType { get; }

  bool Scan(IList<ScanResult> results);
  IList<ILogins> Get(string sqliteDataSource);
  int Insert(string sqliteDataSource, IList<ILogins> logins, Action<int, int> onProgress);
}