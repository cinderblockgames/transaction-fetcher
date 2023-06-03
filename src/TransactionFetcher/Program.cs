using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TransactionFetcher;
using TransactionFetcher.MailWrapper;

Console.WriteLine("Starting up; please wait.");

using IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(Dependencies.Load)
    .Build();

if ("list-folders".Equals(args.FirstOrDefault(), StringComparison.OrdinalIgnoreCase))
{
    var imap = host.Services.GetRequiredService<Imap>();
    Console.WriteLine();
    Console.WriteLine("Available folders:");
    await imap.RunAgainstServer(async mail =>
    {
        var folders = await mail.GetFoldersAsync(mail.PersonalNamespaces.First());
        Console.WriteLine(string.Join(Environment.NewLine, folders));
    });
}
else
{
    // Check for transactions every ten minutes.
    var importer = host.Services.BuildProcessor<TransactionProcessor>(
#if DEBUG
        TimeSpan.FromMinutes(1)
#else
    TimeSpan.FromMinutes(5)
#endif
    );

    var stop = new ManualResetEventSlim();
    AppDomain.CurrentDomain.ProcessExit += (_, _) => stop.Set();
    Console.WriteLine("Running.  Awaiting SIGTERM.");
    stop.Wait();

    Console.WriteLine("Shutting down; please wait.");
    importer.Stop().Wait();
    Console.WriteLine("Shutdown complete.");
}