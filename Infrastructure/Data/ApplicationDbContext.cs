using homework.Core.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace homework.Infrastructure.Data
{
    /// <summary>
    /// the entity framework implementation. Used to communicate with the database
    /// </summary>
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext CreateInstance()
        {
            return new ApplicationDbContext();
        }
    }
}

