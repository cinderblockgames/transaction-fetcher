using System.Globalization;
using HtmlAgilityPack;
using MimeKit;

namespace TransactionFetcher.Readers.Ally;

public class AllyTransactionReader : ITransactionReader
{
    #region " Setup "

    public string Name => "Ally";
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
                   from.Domain.EndsWith("ally.com", StringComparison.OrdinalIgnoreCase))
               && message.HtmlBody.Contains($"*{Options!.LastFour}</td>");
    }

    #endregion

    #region " Read "

    public Transaction Read(MimeMessage message)
    {
        var text = GetRelevantText(message.HtmlBody);

        var amount = NextDecimal(text, "Ally Bank Alert", 3)
                         ?? NextDecimal(text, "Amount");
        return new Transaction
        {
            Account = Options!.AccountId,
            Date = NextDate(text, "Date:") ?? message.Date.Date,
            PayeeName = NextValue(text, "Transaction:") ?? NextValue(text, "To"),
            Amount = IsDebit(message.Subject)
                ? TransactionAmount.Payment(amount)
                : TransactionAmount.Deposit(amount)
        };
    }

    #region " Helpers "

    private bool IsDebit(string subject)
    {
        return subject.Contains("debit", StringComparison.OrdinalIgnoreCase)
            || subject.Contains("payment", StringComparison.OrdinalIgnoreCase);
    }

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