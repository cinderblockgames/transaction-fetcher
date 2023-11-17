using TransactionFetcher.Readers;

namespace TransactionFetcher.ActualWrapper;

public class TransactionEnvelope
{
    public Transaction Transaction { get; }
    public TransactionEnvelope(Transaction transaction) => Transaction = transaction;
}