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
    
    private StoreFlagsRequest Seen { get; }
    
    public TransactionProcessor(TransactionReaders readers, Imap imap, Actual actual, ProcessorOptions options)
    {
        Readers = readers.Instances;
        Imap = imap;
        Actual = actual;

        var flags = MessageFlags.Seen;
        if (options.DeleteAfterProcessing)
        {
            flags |= MessageFlags.Deleted;
        }
        
        Seen = new StoreFlagsRequest(
            StoreAction.Add,
            flags
        ) { Silent = true };
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
                        try
                        {
                            var transaction = reader.Read(message);
                            transaction.ImportedId ??= message.MessageId; // Prevent duplicates.
                            var success = await Actual.AddTransaction(transaction.Account!.Value, transaction);
                            if (success)
                            {
                                await folder.StoreAsync(id, Seen);
                            }
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