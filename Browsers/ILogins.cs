public interface ILogins
{
  string OriginUrl { get; }
  string UsernameElement { get; }
  string UsernameValue { get; }
  string PasswordElement { get; }
  long PasswordType { get; }
  string SignonRealm { get; }
}