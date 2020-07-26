using System;
using System.Collections.Generic;

public static class AppArgs
{
  private static Dictionary<string, string> arguments = new Dictionary<string, string>();

  public static void Parse(string[] args, bool log)
  {
    for (int i = 0; i < args.Length; ++i)
    {
      string arg = args[i];

      string[] splitted = arg.Split('=');
      if (splitted.Length == 1)
      {
        arguments.Add(splitted[0], string.Empty);
      }
      else if (splitted.Length == 2)
      {
        arguments.Add(splitted[0], splitted[1]);
      }
      else
      {
        if (log)
          Console.WriteLine("AppArgs.Parse: strange argument " + arg + " with " + splitted.Length + " splitted vars");
        continue;
      }
    }

    if (arguments.Count > 0)
    {
      int counter = 0;
      foreach (KeyValuePair<string, string> kv in arguments)
      {
        if (log)
          Console.WriteLine("AppArgs.Parse: #" + counter + ": " + kv.Key + "=" + kv.Value);
        counter++;
      }
    }
    else
    {
      if (log)
        Console.WriteLine("AppArgs.Parse: no arguments found");
    }
  }

  public static string GetArgString(string key, string defaultValue)
  {
    if (GetArg(key, out string str))
    {
      return str;
    }
    else
    {
      return defaultValue;
    }
  }

  public static float GetArgFloat(string key, float defaultValue)
  {
    if (GetArg(key, out string str) && float.TryParse(str, out float result))
    {
      return result;
    }
    else
    {
      return defaultValue;
    }
  }

  public static long GetArgLong(string key, long defaultValue)
  {
    if (GetArg(key, out string str) && long.TryParse(str, out long result))
    {
      return result;
    }
    else
    {
      return defaultValue;
    }
  }

  public static int GetArgInt(string key, int defaultValue)
  {
    if (GetArg(key, out string str) && int.TryParse(str, out int result))
    {
      return result;
    }
    else
    {
      return defaultValue;
    }
  }

  public static bool GetArgBool(string key, bool defaultValue)
  {
    if (GetArg(key, out string str))
    {
      str = str.ToLowerInvariant();
      return str == "true" || str == "1" || str == "enabled" || str == "yes";
    }
    else
    {
      return defaultValue;
    }
  }

  public static bool GetArg(string key, out string value)
  {
    return arguments.TryGetValue(key, out value);
  }
}