using MimeKit;

namespace TransactionFetcher.Readers;

public interface ITransactionReader
{
    string Name { get; }
    Type OptionsType { get; }
    
    void Initialize(TransactionReaderOptions options);
    bool CanRead(MimeMessage message);
    Transaction? Read(MimeMessage message);
}