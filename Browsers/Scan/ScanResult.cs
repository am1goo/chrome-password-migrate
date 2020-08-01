public struct ScanResult
{
  public string path;
  public string encryptedKey;

  public ScanResult(string path, string encryptedKey)
  {
    this.path = path;
    this.encryptedKey = encryptedKey;
  }
}