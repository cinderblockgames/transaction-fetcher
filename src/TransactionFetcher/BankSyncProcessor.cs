using TransactionFetcher.ActualWrapper;

namespace TransactionFetcher;

internal class BankSyncProcessor : Processor
{
    private Actual Actual { get; }
    private bool Sync { get; }
    
    public BankSyncProcessor(Actual actual, bool sync)
    {
        Actual = actual;
        Sync = sync;
    }

    protected override async Task Process()
    {
        if (Sync)
        {
            await Actual.BankSync();
        }
    }
}