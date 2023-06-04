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
               && message.HtmlBody.Contains($"<nobr>{Options!.LastFour}</nobr>");
    }

    #endregion

    #region " Read "

    public Transaction Read(MimeMessage message)
    {
        var text = GetRelevantText(message.HtmlBody);

        var transaction = new Transaction
        {
            Account = Options!.AccountId,
            Date = NextDate(text, "Date:"),
            PayeeName = NextValue(text, "Transaction:"),
            AmountInCents = (int)(NextDecimal(text, "Ally Bank Alert", 3) * 100)
        };

        if (IsNegative(message.Subject))
        {
            transaction.AmountInCents *= -1;
        }

        return transaction;
    }

    #region " Helpers "

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
        return decimal.Parse(value, NumberStyles.Currency, Locale);
    }

    private DateTime NextDate(List<string> text, string after, int skip = 1)
    {
        var value = NextValue(text, after, skip);
        return DateTime.ParseExact(value, "M/d/yyyy", Locale, DateTimeStyles.None);
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

    #endregion

    #endregion
}