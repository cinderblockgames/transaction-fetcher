using MailKit;
using MailKit.Search;
using TransactionFetcher.ActualWrapper;
using TransactionFetcher.MailWrapper;
using TransactionFetcher.Readers;

namespace TransactionFetcher;

internal class TransactionProcessor : Processor
{
    private ITransactionReader[] Readers { get; }
    private Imap Imap { get; }
    private Actual Actual { get; }
    
    private StoreFlagsRequest Seen { get; } =
        new StoreFlagsRequest(StoreAction.Add, MessageFlags.Seen) { Silent = true };
    
    public TransactionProcessor(TransactionReaders readers, Imap imap, Actual actual)
    {
        Readers = readers.Instances;
        Imap = imap;
        Actual = actual;
    }

    protected override async Task Process()
    {
        await Imap.RunAgainstFolder(async folder =>
        {
            var ids = await folder.SearchAsync(SearchQuery.NotSeen);
            foreach (var id in ids)
            {
                var message = await folder.GetMessageAsync(id);

                var found = false;
                foreach (var reader in Readers)
                {
                    if (reader.CanRead(message))
                    {
                        found = true;
                        Console.WriteLine($"::: {reader.Name} transaction alert found.");
                        var transaction = reader.Read(message);
                        try
                        {
                            transaction.ImportedId ??= message.MessageId; // Prevent duplicates.
                            await Actual.AddTransaction(transaction.Account!.Value, transaction);
                            await folder.StoreAsync(id, Seen);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(
                                $"{reader.Name} transaction alert '{message.Subject}' unable to be processed.");
                            Console.WriteLine(ex);
                        }
                    }
                }

                if (!found)
                {
                    Console.WriteLine($"Unable to find transaction reader for '{message.Subject}' transaction alert.");
                }
            }
        });
    }
}