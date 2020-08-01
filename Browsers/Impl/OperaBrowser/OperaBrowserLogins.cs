public class OperaBrowserLogins : ILogins
{
  public OperaBrowserLogins() { }

  [Id, Ignore, Column(Name = "id")]
  public long Id { get; private set; }

  [Column(Name = "origin_url")]
  public string OriginUrl { get; private set; }

  [Column(Name = "action_url")]
  public string ActionUrl { get; private set; }

  [Column(Name = "username_element")]
  public string UsernameElement { get; private set; }

  [Column(Name = "username_value")]
  public string UsernameValue { get; private set; }

  [Column(Name = "password_element")]
  public string PasswordElement { get; private set; }

  [Column(Name = "password_value")]
  public byte[] PasswordValue { get; private set; }

  [Column(Name = "submit_element")]
  public string SubmitElement { get; private set; }

  [Column(Name = "signon_realm")]
  public string SignonRealm { get; private set; }

  [Column(Name = "preferred")]
  public long Preferred { get; private set; }

  [Column(Name = "date_created")]
  public long DateCreated { get; private set; }

  [Column(Name = "blacklisted_by_user")]
  public long BlacklistedByUser { get; private set; }

  [Column(Name = "scheme")]
  public long Scheme { get; private set; }

  [Column(Name = "password_type")]
  public long PasswordType { get; private set; }

  [Column(Name = "times_used")]
  public long TimesUsed { get; private set; }

  [Column(Name = "form_data")]
  public byte[] FormData { get; private set; }

  [Column(Name = "date_synced")]
  public long DateSynced { get; private set; }

  [Column(Name = "display_name")]
  public string DisplayName { get; private set; }

  [Column(Name = "icon_url")]
  public string IconUrl { get; private set; }

  [Column(Name = "federation_url")]
  public string FederationUrl { get; private set; }

  [Column(Name = "skip_zero_click")]
  public long SkipZeroClick { get; private set; }

  [Column(Name = "generation_upload_status")]
  public long GenerationUploadStatus { get; private set; }

  [Column(Name = "possible_username_pairs")]
  public byte[] PossibleUsernamePairs { get; private set; }

  [Column(Name = "date_last_used")]
  public long DateLastUsed { get; private set; }

  [Column(Name = "moving_blocked_for")]
  public byte[] MovingBlockedFor { get; private set; }
}