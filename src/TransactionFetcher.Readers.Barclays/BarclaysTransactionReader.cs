using System.Globalization;
using System.Text.RegularExpressions;
using MimeKit;

namespace TransactionFetcher.Readers.Barclays;

public class BarclaysTransactionReader : ITransactionReader
{
    #region " Setup "

    public string Name => "Barclays";
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
                   from.Domain.Equals("services.BarclaysUS.com", StringComparison.OrdinalIgnoreCase)
                   || from.LocalPart.Contains("BarclaysUS", StringComparison.OrdinalIgnoreCase)) // SimpleLogin
               && message.HtmlBody.Contains($">{Options!.LastFour}");
    }
    
    #endregion
    
    #region " Read "

    public Transaction Read(MimeMessage message)
    {
        var match = Regex.Match(
            message.HtmlBody,
            @"Your purchase on (?<date>(?:\d{2}\/){2}\d{4}) for .?(?<amount>[\d., ]+) from (?<payee>.*?) was posted to your (?<account>.*?) account.");

        return new Transaction
        {
            Account = Options!.AccountId,
            Date = DateTime.Parse(match.Groups["date"].Value, Locale),
            PayeeName = match.Groups["payee"].Value,
            Amount = TransactionAmount.Payment(
                decimal.Parse(match.Groups["amount"].Value, NumberStyles.Currency, Locale))
        };
    }
    
    #endregion
}