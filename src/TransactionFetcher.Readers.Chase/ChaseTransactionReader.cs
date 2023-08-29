using System.Globalization;
using HtmlAgilityPack;
using MimeKit;

namespace TransactionFetcher.Readers.Chase;

public class ChaseTransactionReader : ITransactionReader
{
    #region " Setup "

    public string Name => "Chase";
    public Type OptionsType => typeof(LastFourTransactionReaderOptions);
    
    private LastFourTransactionReaderOptions? Options { get; set; }
    private CultureInfo? Locale { get; set; }
    
    public void Initialize(TransactionReaderOptions options, CultureInfo locale)
    {
        Options = (LastFourTransactionReaderOptions)options;
        Locale = locale;
    }
    
    #endregion
    
    #region " CanRead "

    public bool CanRead(MimeMessage message)
    {
        return message.From.OfType<MailboxAddress>().Any(from =>
                   from.Address.Equals("no.reply.alerts@chase.com", StringComparison.OrdinalIgnoreCase))
               && message.HtmlBody.Contains($"(...{Options!.LastFour})");
    }
    
    #endregion
    
    #region " Read "

    public Transaction Read(MimeMessage message)
    {
        var text = GetRelevantText(message.HtmlBody);

        var credit = message.Subject.Contains("credit", StringComparison.OrdinalIgnoreCase);
        
        var amount = NextDecimal(text, credit ? "Credit Amount" : "Amount");
        return new Transaction
        {
            Account = Options!.AccountId,
            Date = NextDate(text, "Date"),
            PayeeName = NextValue(text, "Merchant"),
            Amount = credit
                ? TransactionAmount.Deposit(amount)
                : TransactionAmount.Payment(amount)
        };
    }
    
    #region " Helpers "
    
    private string? NextValue(List<string> text, string after, int skip = 1)
    {
        var index = text.IndexOf(after);
        return index > -1 ? text[index + skip] : null;
    }

    private decimal? NextDecimal(List<string> text, string after, int skip = 1)
    {
        var value = NextValue(text, after, skip);
        return value != null ? decimal.Parse(value, NumberStyles.Currency, Locale) : null;
    }

    private DateTime? NextDate(List<string> text, string after, int skip = 1)
    {
        var value = NextValue(text, after, skip);
        if (value != null)
        {
            value = value.Split(" at ").First();
            return DateTime.ParseExact(value, "MMM d, yyyy", Locale, DateTimeStyles.None);
        }
        return value != null ? DateTime.ParseExact(value, "M/d/yyyy", Locale, DateTimeStyles.None) : null;
    }
    
    private List<string> GetRelevantText(string html)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        return doc.DocumentNode
            .SelectNodes("//table")
            .First()
            .InnerText
             .Split('\n', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .ToList();
    }
    
    #endregion
    
    #endregion
}