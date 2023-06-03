using MailKit;
using MailKit.Search;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TransactionFetcher;
using TransactionFetcher.ActualWrapper;
using TransactionFetcher.MailWrapper;


Console.WriteLine("Starting up; please wait.");

using IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(Dependencies.Load)
    .Build();


var imap = host.Services.GetRequiredService<Imap>();

if ("list-folders".Equals(args.FirstOrDefault(), StringComparison.OrdinalIgnoreCase))
{
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
    var actual = host.Services.GetRequiredService<Actual>();
    var readers = host.Services.GetRequiredService<TransactionReaders>().Instances;

    var seen = new StoreFlagsRequest(StoreAction.Add, MessageFlags.Seen) { Silent = true };

    await imap.RunAgainstFolder(async folder =>
    {
        var ids = await folder.SearchAsync(SearchQuery.NotSeen);
        foreach (var id in ids)
        {
            var message = await folder.GetMessageAsync(id);

            var found = false;
            foreach (var reader in readers)
            {
                if (reader.CanRead(message))
                {
                    found = true;
                    Console.WriteLine($"{reader.Name} transaction alert found.");
                    var transaction = reader.Read(message);
                    if (transaction != null)
                    {
                        try
                        {
                            await actual.AddTransaction(transaction.Account, transaction);
                            await folder.StoreAsync(id, seen);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(
                                $"{reader.Name} transaction alert '{message.Subject}' unable to be processed.");
                            Console.WriteLine(ex);
                        }
                    }
                    else
                    {
                        Console.WriteLine($"{reader.Name} transaction alert '{message.Subject}' unable to be read.");
                    }
                }
            }

            if (!found)
            {
                Console.WriteLine($"Unable to find transaction reader for '{message.Subject}.'");
            }
        }
    });
}