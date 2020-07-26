using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;

public static class SQLiteHelper
{
  public static DataTable SqlReader(SQLiteConnection conn, string sql, Action<SQLiteCommand> onCommand)
  {
    using (SQLiteCommand command = new SQLiteCommand(conn))
    {
      command.CommandText = sql;
      command.CommandType = CommandType.Text;
      if (onCommand != null)
        onCommand(command);

      SQLiteDataReader dataReader = command.ExecuteReader();
      DataTable dt = new DataTable("Sql");
      dt.Load(dataReader);
      return dt;
    }
  }

  public static int SqlNonQuery(SQLiteConnection conn, string sql, Action<SQLiteCommand> onCommand)
  {
    using (SQLiteCommand command = new SQLiteCommand(conn))
    {
      command.CommandText = sql;
      command.CommandType = CommandType.Text;
      if (onCommand != null)
        onCommand(command);

      return command.ExecuteNonQuery();
    }
  }

  public static DataTable GetAllTables(SQLiteConnection conn)
  {
    string sql = @"SELECT name FROM sqlite_master WHERE type ='table' AND name NOT LIKE 'sqlite_%';";
    DataTable dt = SqlReader(conn, sql, null);
    return dt;
  }

  public static DataTable SelectAll(SQLiteConnection conn, string table)
  {
    string sql = string.Format(@"SELECT * FROM {0};", table);
    DataTable dt = SqlReader(conn, sql, null);
    return dt;
  }

  public static int InsertAll(SQLiteConnection conn, string table, IList<SQLiteParameter>[] list, Action<int, int> onProgress)
  {
    int ret = 0;
    int counter = 0;
    foreach (var parameters in list)
    {
      counter++;

      BuildSQLParams(parameters, out string fields, out string values, out SQLiteParameter[] valuesArray);
      string sql = string.Format(@"INSERT INTO {0} ({1}) VALUES ({2});", table, fields, values);
      ret += SqlNonQuery(conn, sql, (x) =>
      {
        x.Parameters.AddRange(valuesArray);
      });

      if (onProgress != null)
        onProgress(counter, list.Length);
    }
    return ret;
  }

  private static void BuildSQLParams(IList<SQLiteParameter> list, out string fields, out string values, out SQLiteParameter[] valuesArray)
  {
    fields = string.Empty;
    values = string.Empty;
    valuesArray = new SQLiteParameter[list.Count];

    for (int i = 0; i < list.Count; ++i)
    {
      SQLiteParameter p = list[i];

      if (i > 0)
      {
        fields += ", ";
        values += ", ";
      }

      string pName = p.ParameterName;
      string @Name = "@" + pName;

      valuesArray[i] = new SQLiteParameter(@Name, p.Value);
      fields += pName;
      values += @Name;
    }
  }

  public static bool CheckValueExists(DataTable dt, string columnName, string value)
  {
    if (!dt.Columns.Contains(columnName))
      return false;

    for (int i = 0; i < dt.Rows.Count; ++i)
    {
      DataRow row = dt.Rows[i];
      string str = row[columnName].ToString();
      if (str == value)
      {
        return true;
      }
    }
    return false;
  }
}