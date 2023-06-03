using Environment = SLSAK.Utilities.Environment;
using File = SLSAK.Docker.IO.File;

namespace TransactionFetcher;

public class EnvironmentVariables
{
    public string ServerUrl { get; }
    public string ServerPassword { get; }
    public string BudgetSyncId { get;  }
    
    public string MailServer { get; }
    public string MailUsername { get; }
    public string MailPassword { get; }
    public string? MailFolder { get; }
    public string MailUseTls { get; }
    public string ImapPort { get; }
    
    public string AccountsFolder { get; set; }
    
    private EnvironmentVariables(
        string serverUrl, string serverPassword, string budgetSyncId,
        string mailServer, string mailUsername, string mailPassword, string? mailFolder, string mailUseTls, string imapPort,
        string accountsFolder)
    {
        ServerUrl = serverUrl;
        ServerPassword = serverPassword;
        BudgetSyncId = budgetSyncId;

        MailServer = mailServer;
        MailUsername = mailUsername;
        MailPassword = mailPassword;
        MailFolder = mailFolder;
        MailUseTls = mailUseTls;
        ImapPort = imapPort;

        AccountsFolder = accountsFolder;
    }

    public static EnvironmentVariables Build()
    {
        var env = Environment.GetEnvironmentVariables(false);

        // -----------------
        //   ACTUAL
        // -----------------
        
        if (!env.TryGetValue("SERVER_URL", out string? serverUrl) || string.IsNullOrWhiteSpace(serverUrl))
        {
            throw new Exception("SERVER_URL must be valued.");
        }
        
        if (!env.TryGetValue("SERVER_PASSWORD", out string? serverPassword) || string.IsNullOrWhiteSpace(serverPassword))
        {
            if (!env.TryGetValue("SERVER_PASSWORD_FILE", out string? serverPasswordFile) ||
                string.IsNullOrWhiteSpace(serverPasswordFile) ||
                !File.Exists(serverPasswordFile))
            {
                throw new Exception("SERVER_PASSWORD or SERVER_PASSWORD_FILE (and related file) must be valued.");
            }

            serverPassword = File.ReadAllText(serverPasswordFile);
            if (string.IsNullOrWhiteSpace(serverPassword))
            {
                throw new Exception("SERVER_PASSWORD or SERVER_PASSWORD_FILE (and related file) must be valued.");
            }
        }
        
        if (!env.TryGetValue("BUDGET_SYNC_ID", out string? budgetSyncId) || string.IsNullOrWhiteSpace(budgetSyncId))
        {
            throw new Exception("BUDGET_SYNC_ID must be valued.");
        }
        
        // -----------------
        //   IMAP
        // -----------------

        if (!env.TryGetValue("MAIL_SERVER", out string? mailServer) || string.IsNullOrWhiteSpace(mailServer))
        {
            throw new Exception("MAIL_SERVER must be valued.");
        }
        
        if (!env.TryGetValue("MAIL_USERNAME", out string? mailUsername) || string.IsNullOrWhiteSpace(mailUsername))
        {
            throw new Exception("MAIL_USERNAME must be valued.");
        }
        
        if (!env.TryGetValue("MAIL_PASSWORD", out string? mailPassword) || string.IsNullOrWhiteSpace(mailPassword))
        {
            if (!env.TryGetValue("MAIL_PASSWORD_FILE", out string? mailPasswordFile) ||
                string.IsNullOrWhiteSpace(mailPasswordFile) ||
                !File.Exists(mailPasswordFile))
            {
                throw new Exception("MAIL_PASSWORD or MAIL_PASSWORD_FILE (and related file) must be valued.");
            }

            mailPassword = File.ReadAllText(mailPasswordFile);
            if (string.IsNullOrWhiteSpace(mailPassword))
            {
                throw new Exception("MAIL_PASSWORD or MAIL_PASSWORD_FILE (and related file) must be valued.");
            }
        }
        
        if (!env.TryGetValue("MAIL_FOLDER", out string? mailFolder) || string.IsNullOrWhiteSpace(mailFolder))
        {
            Console.WriteLine("MAIL_FOLDER not provided; defaulting to inbox.");
        }

        if (!env.TryGetValue("MAIL_USER_TLS", out string? mailUseTls) || string.IsNullOrWhiteSpace(mailUseTls))
        {
            Console.WriteLine("MAIL_USER_TLS not provided; defaulting to false.");
            mailUseTls = "false";
        }
        
        if (!env.TryGetValue("IMAP_PORT", out string? imapPort) || string.IsNullOrWhiteSpace(imapPort))
        {
            Console.WriteLine("IMAP_PORT not provided; defaulting to 143.");
            imapPort = "143";
        }

        // -----------------
        //   ACCOUNTS
        // -----------------

        if (!env.TryGetValue("ACCOUNTS_FOLDER", out string? accountsFolder) || string.IsNullOrWhiteSpace(accountsFolder))
        {
            Console.WriteLine("ACCOUNTS_FOLDER not provided; defaulting to /accounts.");
            accountsFolder = "/accounts";
        }
        
        return new EnvironmentVariables(
            serverUrl, serverPassword, budgetSyncId,
            mailServer, mailUsername, mailPassword, mailFolder, mailUseTls, imapPort,
            accountsFolder);
    }
}