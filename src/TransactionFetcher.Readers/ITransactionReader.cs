using System.Globalization;
using MimeKit;

namespace TransactionFetcher.Readers;

public interface ITransactionReader
{
    string Name { get; }
    Type OptionsType { get; }
    
    void Initialize(TransactionReaderOptions options, CultureInfo locale);
    bool CanRead(MimeMessage message);
    Transaction? Read(MimeMessage message);
}