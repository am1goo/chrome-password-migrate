﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Reflection;

public static class BrowserHelper
{
  public static DirectoryInfo Directory(Environment.SpecialFolder specialFolder)
  {
    string folderPath = Environment.GetFolderPath(specialFolder);
    return new DirectoryInfo(folderPath);
  }

  private static Dictionary<T, PropertyInfo> FindProperties<T>(Type type) where T : Attribute
  {
    Dictionary<T, PropertyInfo> results = new Dictionary<T, PropertyInfo>();

    PropertyInfo[] pis = type.GetProperties();
    foreach (PropertyInfo pi in pis)
    {
      T attr = pi.GetCustomAttribute<T>();
      if (attr == null) continue;

      results.Add(attr, pi);
    }

    return results;
  }

  public static T Copy<T>(ILogins src, IBrowser srcBrowser, string srcEncryptedKey, IBrowser destBrowser, string destEncryptedKey) where T : ILogins
  {
    bool converted = Convert(srcBrowser.supportedEncryption, srcEncryptedKey, src.PasswordValue, destBrowser.supportedEncryption, destEncryptedKey, out byte[] passwordValue);
    if (!converted)
      return default(T);

    Type srcType = src.GetType();
    Type destType = destBrowser.loginsType;

    object destObj = Activator.CreateInstance(destType);
    if (!(destObj is T))
      return default(T);

    T dest = (T)destObj;

    Dictionary<ColumnAttribute, PropertyInfo> srcDict = FindProperties<ColumnAttribute>(srcType);
    Dictionary<ColumnAttribute, PropertyInfo> destDict = FindProperties<ColumnAttribute>(destType);

    foreach (var kv in destDict)
    {
      ColumnAttribute attrDest = kv.Key;
      PropertyInfo piDest = kv.Value;

      if (srcDict.TryGetValue(attrDest, out PropertyInfo piSrc))
      {
        PasswordAttribute pwdAttrSrc = piSrc.GetCustomAttribute<PasswordAttribute>();
        PasswordAttribute pwdAttrDest = piDest.GetCustomAttribute<PasswordAttribute>();

        if (pwdAttrSrc != null && pwdAttrDest != null)
        {
          object value = passwordValue;
          piDest.SetValue(dest, value);
        }
        else
        {
          object value = piSrc.GetValue(src, null);
          piDest.SetValue(dest, value);
        }
      }
    }
    return dest;
  }

  public static bool Convert(EncryptionMode srcMode, string srcEncryptedKey, byte[] srcBytes, EncryptionMode destMode, string destEncryptedKey, out byte[] destBytes)
  {
    string pwd;
    switch (srcMode)
    {
      case EncryptionMode.ChromeV10:
        EncryptHelper.Result decrypted = EncryptHelper.DecryptV10(srcBytes, srcEncryptedKey, out pwd);
        if (decrypted != EncryptHelper.Result.Success)
        {
          destBytes = null;
          return false;
        }
        break;

      default:
        destBytes = null;
        return false;
    }

    switch (destMode)
    {
      case EncryptionMode.ChromeV10:
        EncryptHelper.Result encrypted = EncryptHelper.EncryptV10(pwd, destEncryptedKey, out byte[] passwordValue);
        if (encrypted != EncryptHelper.Result.Success)
        {
          destBytes = null;
          return false;
        }

        destBytes = passwordValue;
        return true;

      default:
        destBytes = null;
        return false;
    }
  }

  public static IList<SQLiteParameter>[] Convert(IList<ILogins> logins)
  {
    IList<SQLiteParameter>[] results = new IList<SQLiteParameter>[logins.Count];
    for (int i = 0; i < logins.Count; ++i)
    {
      results[i] = Convert(logins[i]);
    }
    return results;
  }

  public static IList<SQLiteParameter> Convert(ILogins logins)
  {
    IList<SQLiteParameter> results = new List<SQLiteParameter>();

    Type type = logins.GetType();
    Dictionary<ColumnAttribute, PropertyInfo> dict = FindProperties<ColumnAttribute>(type);
    foreach (var kv in dict)
    {
      ColumnAttribute attr = kv.Key;
      PropertyInfo pi = kv.Value;
      object value = pi.GetValue(logins, null);

      IgnoreAttribute ignore = pi.GetCustomAttribute<IgnoreAttribute>();
      if (ignore != null) continue;

      SQLiteParameter param = new SQLiteParameter(attr.Name, value);
      results.Add(param);
    }

    return results;
  }

  public static IList<ILogins> Parse<T>(DataTable dt) where T : ILogins
  {
    return Parse(dt, typeof(T));
  }

  public static IList<ILogins> Parse(DataTable dt, Type type)
  {
    IList<ILogins> results = new List<ILogins>();

    for (int i = 0; i < dt.Rows.Count; ++i)
    {
      DataRow row = dt.Rows[i];
      ILogins logins = Parse(row, type);
      results.Add(logins);
    }

    return results;
  }

  public static ILogins Parse(DataRow row, Type type)
  {
    object res = Activator.CreateInstance(type);

    if (type.IsClass)
    {
      PropertyInfo[] pis = type.GetProperties();
      foreach (PropertyInfo pi in pis)
      {
        ColumnAttribute attr = pi.GetCustomAttribute<ColumnAttribute>();
        if (attr == null) continue;

        string columnName = attr.Name;
        if (!row.Table.Columns.Contains(columnName))
        {
          Console.WriteLine(string.Format("Parse: missed column name {0}", columnName));
          continue;
        }

        object value = row[columnName];
        if (value is DBNull)
        {
          pi.SetValue(res, null);
        }
        else
        {
          pi.SetValue(res, value);
        }
      }
    }
    else
    {
      TypedReference reference = __makeref(res);

      FieldInfo[] fis = type.GetFields();
      foreach (FieldInfo fi in fis)
      {
        ColumnAttribute attr = fi.GetCustomAttribute<ColumnAttribute>();
        if (attr == null) continue;

        string columnName = attr.Name;
        if (!row.Table.Columns.Contains(columnName))
        {
          Console.WriteLine(string.Format("Parse: missed column name {0}", columnName));
          continue;
        }
        
        object value = row[columnName];
        if (value is DBNull)
        {
          fi.SetValueDirect(reference, null);
        }
        else
        {
          fi.SetValueDirect(reference, value);
        }
      }
    }

    return res as ILogins;
  }
}