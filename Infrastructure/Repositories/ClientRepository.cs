// Infrastructure/Repositories/ClientRepository.cs
using Client;
using homework.Controllers.SearchParams;
using homework.Core.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace homework.Infrastructure.Repositories;

public class ClientRepository : IClientRepository
{
    private readonly ApplicationDbContext _db;
    public ClientRepository(ApplicationDbContext db) => _db = db;

    public async Task AddAsync(Client.Client client) => await _db.Clients.AddAsync(client);

    public async Task<Client.Client?> GetByIdAsync(int id) =>
        await _db.Clients
            .Include(c => c.Addresses)
            .Include(c => c.Accounts)
            .FirstOrDefaultAsync(c => c.Id == id);

    public async Task<List<Client.Client>> GetAllAsync() =>
        await _db.Clients
            .Include(c => c.Addresses)
            .Include(c => c.Accounts)
            .ToListAsync();

    public async Task<(int total, List<Client.Client> results)> SearchAsync(SearchParams searchParams)
    {
        var query = _db.Clients
            .Include(c => c.Addresses)
            .Include(c => c.Accounts)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchParams.search))
            query = query.Where(c =>
                c.FirstName.Contains(searchParams.search) ||
                c.LastName.Contains(searchParams.search)  ||
                c.Email.Contains(searchParams.search)     ||
                c.PersonalId.Contains(searchParams.search));

        query = searchParams.sortBy?.ToLower() switch
        {
            "firstname" => searchParams.sortDir == "desc" ? query.OrderByDescending(c => c.FirstName) : query.OrderBy(c => c.FirstName),
            "lastname"  => searchParams.sortDir == "desc" ? query.OrderByDescending(c => c.LastName)  : query.OrderBy(c => c.LastName),
            "email"     => searchParams.sortDir == "desc" ? query.OrderByDescending(c => c.Email)     : query.OrderBy(c => c.Email),
            _           => query.OrderBy(c => c.Id)
        };

        var total = await query.CountAsync();
        var results = await query
            .AsNoTracking()
            .Skip((searchParams.page - 1) * searchParams.pageSize)
            .Take(searchParams.pageSize)
            .ToListAsync();

        return (total, results);
    }

    public async Task<List<string>> GetSearchHistoryJsonAsync(string userId) =>
        await _db.SearchHistories
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAtUtc)
            .Take(3)
            .Select(x => x.QueryJson!)
            .ToListAsync();

    public void Remove(Client.Client client) => _db.Clients.Remove(client);
    public void RemoveAddresses(IEnumerable<Address> addresses) => _db.Addresses.RemoveRange(addresses);
    public void RemoveAccounts(IEnumerable<Account> accounts) => _db.Accounts.RemoveRange(accounts);

    public async Task AddSearchHistoryAsync(string userId, string queryJson)
    {
        await _db.SearchHistories.AddAsync(new SearchHistory
        {
            UserId       = userId,
            QueryJson    = queryJson,
            CreatedAtUtc = DateTime.UtcNow
        });
    }
}