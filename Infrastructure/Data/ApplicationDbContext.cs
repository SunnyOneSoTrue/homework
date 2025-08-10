using Client;
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
        public DbSet<Client.Client> Clients { get; set; } 
        public DbSet<Account> Accounts { get; set; } 
        public DbSet<Address> Addresses { get; set; }
        // public DbSet<SearchHistory> SearchHistories { get; set; }

        public ApplicationDbContext (DbContextOptions<ApplicationDbContext> options): base(options)
        {
            
        }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            var client = modelBuilder.Entity<Client.Client>();
            client.HasKey(c => c.Id);
            client.Property(c => c.FirstName).HasMaxLength(60).IsRequired();
            client.Property(c => c.LastName).HasMaxLength(60).IsRequired();
            client.Property(c => c.Email).IsRequired();
            client.Property(c => c.PersonalId).HasMaxLength(11).IsRequired(); // <- renamed
            client.HasIndex(c => c.Email).IsUnique();
            client.HasIndex(c => c.PersonalId).IsUnique();

            var addr = modelBuilder.Entity<Address>();
            addr.HasKey(a => a.Id);
            addr.Property(a => a.Country).IsRequired();
            addr.Property(a => a.City).IsRequired();
            addr.Property(a => a.Street).IsRequired();
            addr.Property(a => a.ZipCode).IsRequired();
            addr.HasOne(a => a.Client)
                .WithMany(c => c.Addresses)
                .HasForeignKey(a => a.ClientId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            var account = modelBuilder.Entity<Account>();
            account.HasKey(a => a.Id);
            account.HasOne(a => a.Client)
                .WithMany(c => c.Accounts)
                .HasForeignKey(a => a.ClientId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
        }


    }
}

