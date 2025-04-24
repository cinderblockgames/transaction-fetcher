namespace TransactionFetcher.Readers;

public static class TransactionAmount
{
    public static int? Deposit(decimal? amount)
    {
        return amount.HasValue ? (int)(amount * 100) : null;
    }

    public static int? Payment(decimal? amount)
    {
        return amount.HasValue ? (int)(amount * -100) : null;
    }
}