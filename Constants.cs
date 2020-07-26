using System;
using System.IO;

public class Constants
{
  public static readonly DirectoryInfo APP_DATA_LOCAL = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData));
  public static readonly DirectoryInfo APP_DATA_ROAMING = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));
}