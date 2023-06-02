using MimeKit;

namespace TransactionFetcher.Readers;

public interface ITransactionReader
{
    string Name { get; }
    bool CanRead(MimeMessage message);
    Transaction? Read(MimeMessage message);
}