public interface ILogins
{
  string OriginUrl { get; }
  string UsernameElement { get; }
  string UsernameValue { get; }
  string PasswordElement { get; }
  long PasswordType { get; }
  byte[] PasswordValue { get; }
  string SignonRealm { get; }
}