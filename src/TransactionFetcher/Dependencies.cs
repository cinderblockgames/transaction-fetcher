using System.Globalization;
using Jering.Javascript.NodeJS;
using Microsoft.Extensions.DependencyInjection;
using TransactionFetcher.ActualWrapper;
using TransactionFetcher.MailWrapper;

namespace TransactionFetcher;

public static class Dependencies
{
    public static void Load(IServiceCollection services)
    {
        var env = EnvironmentVariables.Build();

        // Node.
        services.AddNodeJS();
        services.Configure<NodeJSProcessOptions>(options =>
            options.ProjectPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "js"));
        services.Configure<OutOfProcessNodeJSServiceOptions>(options =>
        {
            // Fail faster.
            options.TimeoutMS = 5000;
            
            // Don't retry; just makes the logs harder to understand.
            // The app will retry every tick anyway; doesn't have to happen immediately.
            options.NumConnectionRetries = 0;
            options.NumRetries = 0;
            options.NumProcessRetries = 0;
        });
        
        // Actual.
        services.AddSingleton(new ActualWrapper.ConnectionInfo
        {
            ServerUrl = env.ServerUrl,
            ServerPassword = env.ServerPassword,
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
        services.AddSingleton(provider =>
            new TransactionReaders(env.AccountsFolder, provider.GetRequiredService<CultureInfo>()));
        services.AddSingleton<TransactionProcessor>();
    }
}