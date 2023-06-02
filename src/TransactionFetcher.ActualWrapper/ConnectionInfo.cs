namespace TransactionFetcher.ActualWrapper;

public class ConnectionInfo
{
    public string? ServerUrl { get; set; }
    public string? ServerPassword { get; set; }
    public Guid BudgetSyncId { get; set; }
}