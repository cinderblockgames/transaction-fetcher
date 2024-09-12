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
               && message.HtmlBody.Contains($"Account Ending: {Options!.LastFive}", StringComparison.OrdinalIgnoreCase);
    }

    #endregion

    #region " Read "

    public Transaction Read(MimeMessage message)
    {
        return TryReadCredit(message) 
               ?? TryReadDebit(message)
               ?? UnknownTransaction(message);
    }

    private Transaction? TryReadCredit(MimeMessage message)
    {
        if (message.Subject.Contains("credit", StringComparison.OrdinalIgnoreCase))
        {
            var text = GetRelevantText(message.HtmlBody);
            
            return new Transaction
            {
                Account = Options!.AccountId,
                Date = DateTime.Parse(text.Skip(7).First()),
                PayeeName = text.Skip(5).First(),
                Amount = TransactionAmount.Deposit(
                    decimal.Parse(
                        text.Skip(6).First().Replace("-", ""),
                        NumberStyles.Currency,
                        Locale)),
                Cleared = false
            };
        }

        return null;
    }

    private Transaction? TryReadDebit(MimeMessage message)
    {
        if (message.Subject.Equals("Large Purchase Approved", StringComparison.OrdinalIgnoreCase))
        {
            var text = GetRelevantText(message.HtmlBody);

            return new Transaction
            {
                Account = Options!.AccountId,
                Date = DateTime.Parse(text.Skip(9).First()),
                PayeeName = text.Skip(7).First(),
                Amount = TransactionAmount.Payment(
                    decimal.Parse(
                        text.Skip(8).First().Replace("*", ""),
                        NumberStyles.Currency,
                        Locale)),
                Cleared = false
            };
        }

        return null;
    }

    private Transaction UnknownTransaction(MimeMessage message)
    {
        return new Transaction
        {
            Account = Options!.AccountId,
            Date = message.Date.Date,
            Notes = "Unknown transaction.",
            Cleared = false
        };
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
            .ToList();
    }

    #endregion

    #endregion
}