using System.Globalization;
using System.Text.RegularExpressions;
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
               && message.HtmlBody.Contains($"{Options!.LastFour}");
    }

    #endregion

    #region " Read "

    public Transaction Read(MimeMessage message)
    {
        return TryReadDebit(message)
               ?? ReadCredit(message);
    }

    #region " Helpers "

    private Transaction? TryReadDebit(MimeMessage message)
    {
        if (IsDebit(message.Subject))
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(message.HtmlBody);

            var text = doc.DocumentNode
                .SelectNodes("//td")
                .Select(node => node.InnerText)
                .Select(text => string.IsNullOrWhiteSpace(text) ? null : text) // Nullify whitespace.
                .ToList();
            
            var amount = NextDecimal(text, "Amount");
            return new Transaction
            {
                Account = Options!.AccountId,
                Date = NextDate(text, "Date:") ?? message.Date.Date,
                PayeeName = NextValue(text, "Transaction source") ?? NextValue(text, "Transaction"),
                Amount = TransactionAmount.Payment(amount),
                Cleared = false
            };
        }

        return null;
    }

    private Transaction ReadCredit(MimeMessage message)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(message.HtmlBody);

        var text = doc.DocumentNode
            .SelectNodes("//td")
            .SelectMany(node => node.InnerText?.Split(' '))
            .Where(text => !string.IsNullOrWhiteSpace(text))
            .ToList();
        
        var amount = NextDecimal(text, Options!.LastFour, 3);
        return new Transaction
        {
            Account = Options!.AccountId,
            Date = message.Date.Date,
            Amount = TransactionAmount.Deposit(amount),
            Cleared = false
        };
    }

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
        if (value != null)
        {
            value = Regex.Match(value, @"\$[\d,]+\.\d{2}").Value;
            if (!string.IsNullOrWhiteSpace(value))
            {
                return decimal.Parse(value, NumberStyles.Currency, Locale);
            }
        }

        return null;
    }

    private DateTime? NextDate(List<string> text, string after, int skip = 1)
    {
        var value = NextValue(text, after, skip);
        return value != null ? DateTime.ParseExact(value, "M/d/yyyy", Locale, DateTimeStyles.None) : null;
    }

    #endregion

    #endregion
}