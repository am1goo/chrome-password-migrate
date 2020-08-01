public struct BrowserScanResult
{
  public IBrowser browser;
  public ScanResult scan;

  public BrowserScanResult(IBrowser browser, ScanResult scan)
  {
    this.browser = browser;
    this.scan = scan;
  }
}