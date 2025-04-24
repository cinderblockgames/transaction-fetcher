using System.Runtime.InteropServices.ComTypes;
using Environment = SLSAK.Utilities.Environment;
using File = SLSAK.Docker.IO.File;

namespace TransactionFetcher;

public class EnvironmentVariables
{
    public string ApiUrl { get; }
    public string ApiKey { get; }
    public string BudgetSyncId { get; }
    public string BankSync { get; }

    public string MailServer { get; }
    public string MailUsername { get; }
    public string MailPassword { get; }
    public string? MailFolder { get; }
    public string MailUseTls { get; }
    
    public string ImapPort { get; }
    public string PollIntervalSeconds { get; }
    
    public string DeleteAfterProcessing { get; }

    public string AccountsPath { get; }
    public string Locale { get; }

    private EnvironmentVariables(
        string apiUrl, string apiKey, string budgetSyncId, string bankSync,
        string mailServer, string mailUsername, string mailPassword, string? mailFolder, string mailUseTls,
        string imapPort, string pollIntervalSeconds,
        string deleteAfterProcessing,
        string accountsPath, string locale)
    {
        ApiUrl = apiUrl;
        ApiKey = apiKey;
        BudgetSyncId = budgetSyncId;
        BankSync = bankSync;

        MailServer = mailServer;
        MailUsername = mailUsername;
        MailPassword = mailPassword;
        MailFolder = mailFolder;
        MailUseTls = mailUseTls;
        
        ImapPort = imapPort;
        PollIntervalSeconds = pollIntervalSeconds;

        DeleteAfterProcessing = deleteAfterProcessing;
        
        AccountsPath= accountsPath;
        Locale = locale;

    }

    public static EnvironmentVariables Build()
    {
        var env = Environment.GetEnvironmentVariables(false);

        // -----------------
        //   ACTUAL
        // -----------------

        if (!env.TryGetValue("API_URL", out string? apiUrl) || string.IsNullOrWhiteSpace(apiUrl))
        {
            throw new Exception("API_URL must be valued.");
        }

        if (!env.TryGetValue("API_KEY", out string? apiKey) ||
            string.IsNullOrWhiteSpace(apiKey))
        {
            if (!env.TryGetValue("API_KEY_FILE", out string? apiKeyFile) ||
                string.IsNullOrWhiteSpace(apiKeyFile) ||
                !File.Exists(apiKeyFile))
            {
                throw new Exception("API_KEY or API_KEY_FILE (and related file) must be valued.");
            }

            apiKey = File.ReadAllText(apiKeyFile);
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                throw new Exception("API_KEY or API_KEY_FILE (and related file) must be valued.");
            }
        }

        if (!env.TryGetValue("BUDGET_SYNC_ID", out string? budgetSyncId) || string.IsNullOrWhiteSpace(budgetSyncId))
        {
            throw new Exception("BUDGET_SYNC_ID must be valued.");
        }
        
        if (!env.TryGetValue("BANK_SYNC", out string? bankSync) || string.IsNullOrWhiteSpace(bankSync))
        {
            Console.WriteLine("BANK_SYNC not provided; defaulting to false.");
            bankSync = "false";
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
        
        if (!env.TryGetValue("DELETE_AFTER_PROCESSING", out string? deleteAfterProcessing) ||
            string.IsNullOrEmpty(deleteAfterProcessing))
        {
            Console.WriteLine("DELETE_AFTER_PROCESSING not provided; defaulting to false.");
            deleteAfterProcessing = "false";
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
            apiUrl, apiKey, budgetSyncId, bankSync,
            mailServer, mailUsername, mailPassword, mailFolder, mailUseTls,
            imapPort, pollIntervalSeconds,
            deleteAfterProcessing,
            accountsPath, locale);
    }
}
