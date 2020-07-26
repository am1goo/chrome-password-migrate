using System;
using System.Collections.Generic;
using System.Data;

public interface IBrowser
{
  string name { get; }
  string loginsTable { get; }
  Type loginsType { get; }

  bool Scan(IList<string> results);

  IList<ILogins> Get(string sqliteDataSource);
  int Insert(string sqliteDataSource, IList<ILogins> logins, Action<int, int> onProgress);
}