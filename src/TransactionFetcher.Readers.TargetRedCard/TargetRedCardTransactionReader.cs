using System.Globalization;
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
        var match = Regex.Match(
            message.HtmlBody,
            @"A transaction of .?(?<amount>[\d., ]+) at (?<payee>[\w ]+) has posted");

        var transaction = new Transaction
        {
            Account = Options!.AccountId,
            Date = message.Date.Date,
            PayeeName = match.Groups["payee"].Value,
            AmountInCents = (int)(decimal.Parse(match.Groups["amount"].Value, NumberStyles.Currency, Locale) * 100)
        };

        return transaction;
    }

    #endregion
}