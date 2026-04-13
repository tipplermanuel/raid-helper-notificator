namespace RaidNotificator;

public enum DiffType
{
    SignedOut,
    SignedIn,
    Late
    
}

public class RegistrationDiff
{
    public DiffType Type { get; set; }
    public string Username { get; set; }
    public string OldClass { get; set; }
    public string Class { get; set; }
    public string Role { get; set; }
    public int Number { get; set; }
}