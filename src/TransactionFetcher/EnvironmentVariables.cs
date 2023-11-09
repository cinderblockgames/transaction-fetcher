using System.Runtime.InteropServices.ComTypes;
using Environment = SLSAK.Utilities.Environment;
using File = SLSAK.Docker.IO.File;

namespace TransactionFetcher;

public class EnvironmentVariables
{
    public string ServerUrl { get; }
    public string ServerPassword { get; }
    public string BudgetSyncId { get; }
    public string DataPath { get; }

    public string MailServer { get; }
    public string MailUsername { get; }
    public string MailPassword { get; }
    public string? MailFolder { get; }
    public string MailUseTls { get; }
    public string ImapPort { get; }
    public string PollIntervalSeconds { get; }

    public string AccountsPath { get; }
    public string Locale { get; }

    private EnvironmentVariables(
        string serverUrl, string serverPassword, string budgetSyncId, string dataPath,
        string mailServer, string mailUsername, string mailPassword, string? mailFolder, string mailUseTls,
        string imapPort, string pollIntervalSeconds,
        string accountsPath, string locale)
    {
        ServerUrl = serverUrl;
        ServerPassword = serverPassword;
        BudgetSyncId = budgetSyncId;
        DataPath = dataPath;

        MailServer = mailServer;
        MailUsername = mailUsername;
        MailPassword = mailPassword;
        MailFolder = mailFolder;
        MailUseTls = mailUseTls;
        ImapPort = imapPort;
        PollIntervalSeconds = pollIntervalSeconds;

        AccountsPath= accountsPath;
        Locale = locale;
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

        if (!env.TryGetValue("SERVER_PASSWORD", out string? serverPassword) ||
            string.IsNullOrWhiteSpace(serverPassword))
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
        
        if (!env.TryGetValue("DATA_PATH", out string? dataPath) || string.IsNullOrWhiteSpace(dataPath))
        {
            Console.WriteLine("DATA_PATH not provided; defaulting to /data.");
            dataPath = "/data";
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

        if (!env.TryGetValue("MAIL_USE_TLS", out string? mailUseTls) || string.IsNullOrWhiteSpace(mailUseTls))
        {
            Console.WriteLine("MAIL_USE_TLS not provided; defaulting to false.");
            mailUseTls = "false";
        }

        if (!env.TryGetValue("IMAP_PORT", out string? imapPort) || string.IsNullOrWhiteSpace(imapPort))
        {
            Console.WriteLine("IMAP_PORT not provided; defaulting to 143.");
            imapPort = "143";
        }

        if (!env.TryGetValue("POLL_INTERVAL_SECONDS", out string? pollIntervalSeconds) ||
            string.IsNullOrEmpty(pollIntervalSeconds))
        {
            Console.WriteLine("POLL_INTERVAL_SECONDS not provided; defaulting to 300.");
            pollIntervalSeconds = "300";
        }

        // -----------------
        //   ACCOUNTS
        // -----------------

        if (!env.TryGetValue("ACCOUNTS_PATH", out string? accountsPath) ||
            string.IsNullOrWhiteSpace(accountsPath))
        {
            Console.WriteLine("ACCOUNTS_PATH not provided; defaulting to /accounts.");
            accountsPath = "/accounts";
        }

        if (!env.TryGetValue("LOCALE", out string? locale) || string.IsNullOrWhiteSpace(locale))
        {
            Console.WriteLine("LOCALE not provided; defaulting to en-US.");
            locale = "en-US";
        }

        return new EnvironmentVariables(
            serverUrl, serverPassword, budgetSyncId, dataPath,
            mailServer, mailUsername, mailPassword, mailFolder, mailUseTls,
            imapPort, pollIntervalSeconds,
            accountsPath, locale);
    }
}
