using Client;
using Core.Entities;
using homework.Core.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

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
        public DbSet<SearchHistory> SearchHistories { get; set; }
        
        private readonly SeedRolesSettings.SeedRolesSettings _roleSettings;
        private readonly SeedAdminSettings.SeedAdminSettings _adminSettings;

        public ApplicationDbContext (DbContextOptions<ApplicationDbContext> options, 
                                    IOptions<SeedRolesSettings.SeedRolesSettings> roleSettings,
                                    IOptions<SeedAdminSettings.SeedAdminSettings> adminSettings): base(options)
        {
            _roleSettings = roleSettings.Value;
            _adminSettings = adminSettings.Value;
        }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            var client = modelBuilder.Entity<Client.Client>();
            client.HasKey(c => c.Id);
            client.Property(c => c.FirstName).HasMaxLength(60).IsRequired();
            client.Property(c => c.LastName).HasMaxLength(60).IsRequired();
            client.Property(c => c.Email).IsRequired();
            client.Property(c => c.PersonalId).HasMaxLength(11).IsRequired();
            client.HasIndex(c => c.Email).IsUnique();
            client.HasIndex(c => c.PersonalId).IsUnique();
            client.Property(c => c.ProfilePhoto).IsRequired(false);

            var addr = modelBuilder.Entity<Address>();
            addr.HasKey(a => a.Id);
            addr.Property(a => a.Country).IsRequired();
            addr.Property(a => a.City).IsRequired();
            addr.Property(a => a.Street).IsRequired();
            addr.Property(a => a.ZipCode).IsRequired();
            addr.HasOne(a => a.Client).WithMany(c => c.Addresses).HasForeignKey(a => a.ClientId).IsRequired().OnDelete(DeleteBehavior.Cascade);

            var account = modelBuilder.Entity<Account>();
            account.HasKey(a => a.Id);
            account.Property(a => a.AccountNumber).IsRequired();
            account.HasOne(a => a.Client).WithMany(c => c.Accounts).HasForeignKey(a => a.ClientId).IsRequired().OnDelete(DeleteBehavior.Cascade);

            // Seed roles
            var roles = _roleSettings.Roles
                .Select((name, i) => new IdentityRole
                {
                    Id = (i + 1).ToString(),
                    Name = name,
                    NormalizedName = name.ToUpper()
                }).ToArray();

            modelBuilder.Entity<IdentityRole>().HasData(roles);

            // Seed admin user
            const string adminId = "admin-seed-id";

            var adminUser = new ApplicationUser
            {
                Id                 = adminId,
                UserName           = _adminSettings.Email,
                NormalizedUserName = _adminSettings.Email.ToUpper(),
                Email              = _adminSettings.Email,
                NormalizedEmail    = _adminSettings.Email.ToUpper(),
                EmailConfirmed     = true,
                FullName           = "Admin",
                RoleDisplayName    = _adminSettings.Role,
                DateOfBirth        = new DateTime(1820, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                SecurityStamp      = "admin-security-stamp"
            };

            adminUser.PasswordHash = new PasswordHasher<ApplicationUser>()
                .HashPassword(adminUser, _adminSettings.Password);

            modelBuilder.Entity<ApplicationUser>().HasData(adminUser);

            var adminRoleId = roles.First(r => r.Name == _adminSettings.Role).Id;

            modelBuilder.Entity<IdentityUserRole<string>>().HasData(new IdentityUserRole<string>
            {
                UserId = adminId,
                RoleId = adminRoleId
            });
        }
        
        
    }
}

