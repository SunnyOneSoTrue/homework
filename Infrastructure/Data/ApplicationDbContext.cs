using homework.Core.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace homework.Infrastructure.Data
{
    /// <summary>
    /// Represents the Entity Framework Core database context for the application.
    /// Integrates ASP.NET Core Identity for user and role management.
    /// will store domain entities such as clients, accounts, and search history.
    /// </summary>
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public DbSet<Client> Clients { get; set; } //not implemented yet
        public DbSet<Account> Accounts { get; set; } //not implemented yet
        public DbSet<SearchHistory> SearchHistories { get; set; } //not implemented yet

        public ApplicationDbContext (DbContextOptions<ApplicationDbContext> options): base(options)
        {
            
        }
    }
}

