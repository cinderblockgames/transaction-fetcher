namespace TransactionFetcher.MailWrapper;

public class ConnectionInfo
{
    public string? Server { get; set; }
    public string? Username { get; set; }
    public string? Password { get; set; }
    public string? Folder { get; set; }
    public bool UseTls { get; set; }
    public int ImapPort { get; set; } 
}