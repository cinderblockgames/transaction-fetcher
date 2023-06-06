﻿using System.Globalization;
using System.Text.RegularExpressions;
using MimeKit;

namespace TransactionFetcher.Readers.USBank;

public class USBankTransactionReader : ITransactionReader
{
    #region " Setup "

    public string Name => "USBank";
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
                   from.Domain.Equals("notifications.usbank.com", StringComparison.OrdinalIgnoreCase))
               && message.HtmlBody.Contains($"your account ending in {Options!.LastFour}");
    }
    
    #endregion
    
    #region " Read "

    public Transaction Read(MimeMessage message)
    {
        var amountMatch = Regex.Match(
            message.HtmlBody,
            @"{ POST--> of .?(?<amount>[\d., ]+)<!-- `dollar_string`--><!--POST }");
        var dateMatch = Regex.Match(
            message.HtmlBody,
            @"{ POST--> on (?<date>(\d{2}\/){2}\d{4})<!-- `datePost`--><!--POST }");

        return new Transaction
        {
            Account = Options!.AccountId,
            Date = DateTime.Parse(dateMatch.Groups["date"].Value, Locale),
            Amount = TransactionAmount.Payment(
                decimal.Parse(amountMatch.Groups["amount"].Value, NumberStyles.Currency, Locale))
        };
    }
    
    #endregion
}