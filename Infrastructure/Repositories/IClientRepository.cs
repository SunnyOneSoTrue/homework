using Client;
using homework.Controllers.SearchParams;


namespace homework.Infrastructure.Repositories;

public interface IClientRepository
{
    Task AddAsync(Client.Client client);
    Task<Client.Client?> GetByIdAsync(int id);
    Task<List<Client.Client>> GetAllAsync();
    Task<(int total, List<Client.Client> results)> SearchAsync(SearchParams searchParams);
    Task<List<string>> GetSearchHistoryJsonAsync(string userId);
    void Remove(Client.Client client);
    void RemoveAddresses(IEnumerable<Address> addresses);
    void RemoveAccounts(IEnumerable<Account> accounts);
    Task AddSearchHistoryAsync(string userId, string queryJson);
}