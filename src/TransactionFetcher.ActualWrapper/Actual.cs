using System.Net.Http.Json;
using TransactionFetcher.Readers;

namespace TransactionFetcher.ActualWrapper;

public class Actual
{
    
    #region " Constructor, Private Properties "
    
    private HttpClient Api { get; }

    public Actual(ConnectionInfo connectionInfo)
    {
        Api = new HttpClient()
        {
            BaseAddress = new Uri(
                new Uri(connectionInfo.ApiUrl!),
                $"v1/budgets/{connectionInfo.BudgetSyncId}/accounts/"
            )
        };
        Api.DefaultRequestHeaders.Add("X-API-KEY", connectionInfo.ApiKey);
    }
    
    #endregion

    public async Task<bool> AddTransaction(Guid accountId, Transaction transaction)
    {
        return await Process(() => Api.PostAsJsonAsync(
            $"{accountId}/transactions",
            new TransactionEnvelope(transaction)
        ));
    }

    public async Task<bool> BankSync()
    {
        return await Process(() => Api.PostAsync("banksync", null));
    }

    #region " Process "
    
    private async Task<bool> Process(Func<Task<HttpResponseMessage>> call)
    {
        var response = await call();
        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine(await response.RequestMessage.Content.ReadAsStringAsync());
            Console.WriteLine($"    [{(int)response.StatusCode} {response.StatusCode}] {await response.Content.ReadAsStringAsync()}");
        }

        return response.IsSuccessStatusCode;
    }
    
    #endregion

}