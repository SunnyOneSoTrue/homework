// Infrastructure/UnitOfWork/IUnitOfWork.cs
using homework.Infrastructure.Repositories;

namespace homework.Infrastructure.UnitOfWork;

public interface IUnitOfWork
{
    IClientRepository Clients { get; }
    Task<int> SaveChangesAsync();
}