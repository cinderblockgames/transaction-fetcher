using System.Globalization;
using System.Net;
using System.Text.RegularExpressions;
using MimeKit;

namespace TransactionFetcher.Readers.TargetRedCard;

public class TargetRedCardTransactionReader : ITransactionReader
{
    #region " Setup "

    public string Name => "Target RedCard";
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
                   from.Domain.Equals("myredcard.target.com", StringComparison.OrdinalIgnoreCase))
               && message.HtmlBody.Contains($"ending in {Options!.LastFour}");
    }

    #endregion

    #region " Read "

    public Transaction Read(MimeMessage message)
    {
        return TryReadPayment(message)
               ?? TryReadCredit(message)
               ?? ReadDebit(message);
    }

    private Transaction? TryReadPayment(MimeMessage message)
    {
        if (message.Subject.Equals("Thanks for your payment", StringComparison.OrdinalIgnoreCase))
        {
            var match = Regex.Match(
                message.HtmlBody,
                @"A payment of .?(?<amount>[\d., ]+) has recently posted");

            var transaction = new Transaction
            {
                Account = Options!.AccountId,
                Date = message.Date.Date,
                Amount = TransactionAmount.Deposit(
                    decimal.Parse(match.Groups["amount"].Value, NumberStyles.Currency, Locale))
            };

            return transaction;
        }

        return null;
    }

    private Transaction? TryReadCredit(MimeMessage message)
    {
        if (message.Subject.Equals("A credit posted to your account", StringComparison.OrdinalIgnoreCase))
        {
            var transaction = new Transaction
            {
                Account = Options!.AccountId,
                Date = message.Date.Date,
                Notes = "Unknown credit; check card for details.  (May be card payment.)"
            };

            return transaction;
        }

        return null;
    }

    private Transaction ReadDebit(MimeMessage message)
    {
        var match = Regex.Match(
            message.HtmlBody,
            @"A transaction of .?(?<amount>[\d., ]+) at (?<payee>[\w ]+) has posted");

        var transaction = new Transaction
        {
            Account = Options!.AccountId,
            Date = message.Date.Date,
            PayeeName = match.Groups["payee"].Value,
            Amount = TransactionAmount.Payment(
                decimal.Parse(match.Groups["amount"].Value, NumberStyles.Currency, Locale))
        };

        return transaction;
    }

    #endregion
}