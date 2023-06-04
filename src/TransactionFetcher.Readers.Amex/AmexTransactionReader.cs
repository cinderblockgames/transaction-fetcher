using System.Globalization;
using HtmlAgilityPack;
using MimeKit;

namespace TransactionFetcher.Readers.Amex;

public class AmexTransactionReader : ITransactionReader
{
    #region " Setup "

    public string Name => "American Express";
    public Type OptionsType => typeof(LastFiveTransactionReaderOptions);

    private LastFiveTransactionReaderOptions? Options { get; set; }
    private CultureInfo? Locale { get; set; }

    public void Initialize(TransactionReaderOptions options, CultureInfo locale)
    {
        Options = (LastFiveTransactionReaderOptions)options;
        Locale = locale;
    }

    #endregion

    #region " CanRead "

    public bool CanRead(MimeMessage message)
    {
        return message.From.OfType<MailboxAddress>().Any(from =>
                   from.Domain.Equals("welcome.americanexpress.com", StringComparison.OrdinalIgnoreCase))
               && message.Subject.Equals("Large Purchase Approved", StringComparison.OrdinalIgnoreCase)
               && message.HtmlBody.Contains($"<b>{Options!.LastFive}</b>");
    }

    #endregion

    #region " Read "

    public Transaction Read(MimeMessage message)
    {
        var text = GetRelevantText(message.HtmlBody);

        var transaction = new Transaction
        {
            Account = Options!.AccountId,
            Date = DateTime.Parse(text.Last()),
            PayeeName = text.First(),
            AmountInCents = (int)
                (decimal.Parse(text.Skip(1).First().Replace("*", ""), NumberStyles.Currency, Locale) * -100)
        };

        return transaction;
    }

    #region " Helpers "

    private List<string> GetRelevantText(string html)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        return doc.DocumentNode
            .SelectNodes("//p")
            .Select(node => node.InnerText)
            .Where(text => !string.IsNullOrWhiteSpace(text))
            .Skip(7).Take(3)
            .ToList();
    }

    #endregion

    #endregion
}