namespace TransactionFetcher.ActualWrapper;

public class ConnectionInfo
{
    public string? ApiUrl { get; set; }
    public string? ApiKey { get; set; }
    public Guid BudgetSyncId { get; set; }
}