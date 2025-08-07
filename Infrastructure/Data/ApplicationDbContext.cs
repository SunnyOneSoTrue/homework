using Core.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data
{
    /// <summary>
    /// Represents the Entity Framework Core database context for the application.
    /// Integrates ASP.NET Core Identity for user and role management.
    /// will store domain entities such as clients, accounts, and search history.
    /// </summary>
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        //Todo: implement these
        // public DbSet<Client> Clients { get; set; } 
        // public DbSet<Account> Accounts { get; set; } 
        // public DbSet<SearchHistory> SearchHistories { get; set; }

        public ApplicationDbContext (DbContextOptions<ApplicationDbContext> options): base(options)
        {
            
        }
    }
}

