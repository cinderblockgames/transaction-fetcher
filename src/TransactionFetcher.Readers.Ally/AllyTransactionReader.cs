using System.Globalization;
using HtmlAgilityPack;
using MimeKit;

namespace TransactionFetcher.Readers.Ally;

public class AllyTransactionReader : ITransactionReader
{
    private AllyTransactionReaderOptions? Options { get; set; }

    public void Initialize(TransactionReaderOptions options)
    {
        Options = (AllyTransactionReaderOptions)options;
    }

    public string Name => "Ally";
    public Type OptionsType => typeof(AllyTransactionReaderOptions);

    public bool CanRead(MimeMessage message)
    {
        return message.From.OfType<MailboxAddress>().Any(from =>
                   from.Domain.EndsWith("ally.com", StringComparison.OrdinalIgnoreCase))
               && message.HtmlBody.Contains($"<nobr>{Options!.LastFour}</nobr>");
    }

    public Transaction? Read(MimeMessage message)
    {
        var text = GetRelevantText(message.HtmlBody);
        Console.WriteLine(text); // TODO: REMOVE
        
        var transaction = new Transaction { Account = Options!.AccountId };
        transaction.Date = NextDate(text, "Date:");
        transaction.PayeeName = NextValue(text, "Transaction:");
        transaction.AmountInCents = (int)(NextDecimal(text, "Ally Bank Alert", 3) * 100);

        if (IsNegative(message.Subject))
        {
            transaction.AmountInCents *= -1;
        }
        
        return transaction;
    }

    private bool IsNegative(string subject)
    {
        return subject.Contains("debit", StringComparison.OrdinalIgnoreCase);
    }

    private string NextValue(List<string> text, string after, int skip = 1)
    {
        var index = text.IndexOf(after);
        return text[index + skip];
    }
    
    private decimal NextDecimal(List<string> text, string after, int skip = 1)
    {
        var value = NextValue(text, after, skip);
        Console.WriteLine($"decimal: {value}"); // TODO: REMOVE
        return decimal.Parse(value, NumberStyles.Currency);
    }
    
    private DateTime NextDate(List<string> text, string after, int skip = 1)
    {
        var value = NextValue(text, after, skip);
        Console.WriteLine($"date: {value}"); // TODO: REMOVE
        return DateTime.ParseExact(value, "M/d/yyyy", CultureInfo.CurrentCulture, DateTimeStyles.None);
    }
    
    private List<string> GetRelevantText(string html)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        return doc.DocumentNode
            .SelectNodes("//table")
            .First()
            .InnerText
            .Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .ToList();
    }
}