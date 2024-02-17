using System.Globalization;
using Microsoft.Extensions.DependencyInjection;
using TransactionFetcher.ActualWrapper;
using TransactionFetcher.MailWrapper;

namespace TransactionFetcher;

public static class Dependencies
{
    public static void Load(IServiceCollection services)
    {
        var env = EnvironmentVariables.Build();

        // Actual.
        services.AddSingleton(new ActualWrapper.ConnectionInfo
        {
            ApiUrl = env.ApiUrl,
            ApiKey = env.ApiKey,
            BudgetSyncId = Guid.Parse(env.BudgetSyncId)
        });
        services.AddSingleton<Actual>();
        
        // Mail.
        services.AddSingleton(new MailWrapper.ConnectionInfo
        {
            Server = env.MailServer,
            Username = env.MailUsername,
            Password = env.MailPassword,
            Folder = env.MailFolder,
            UseTls = bool.Parse(env.MailUseTls),
            ImapPort = int.Parse(env.ImapPort)
        });
        services.AddSingleton<Imap>();

        // Transaction readers.
        services.AddSingleton(new CultureInfo(env.Locale));
        services.AddSingleton(new PollInterval { Interval = TimeSpan.FromSeconds(int.Parse(env.PollIntervalSeconds)) });
        services.AddSingleton(new ProcessorOptions { DeleteAfterProcessing = bool.Parse(env.DeleteAfterProcessing) });
        services.AddSingleton(provider =>
            new TransactionReaders(env.AccountsPath, provider.GetRequiredService<CultureInfo>()));
        services.AddSingleton<TransactionProcessor>();
    }
}