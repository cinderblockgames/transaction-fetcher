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
            options.TimeoutMS = 5000); // Fail faster.
        
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
        services.AddSingleton(new TransactionReaders(env.AccountsFolder));
        services.AddSingleton<TransactionProcessor>();
    }
}