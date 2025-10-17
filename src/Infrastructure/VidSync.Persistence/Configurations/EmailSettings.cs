namespace VidSync.Persistence.Configurations;

public class EmailSettings
{
    public string Host { get; set; } = null!;
    public int Port { get; set; }
    public string NetworkCredentialMail { get; set; } = null!;
    public string NetworkCredentialPassword { get; set; } = null!;
    public string SenderMail { get; set; } = null!;
    public string SenderName { get; set; } = null!;
}
