// Infrastructure/UnitOfWork/UnitOfWork.cs
using Infrastructure.Data;
using homework.Infrastructure.Repositories;

namespace homework.Infrastructure.UnitOfWork;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _db;
    public IClientRepository Clients { get; }

    public UnitOfWork(ApplicationDbContext db, IClientRepository clients)
    {
        _db = db;
        Clients = clients;
    }

    public async Task<int> SaveChangesAsync() => await _db.SaveChangesAsync();
}