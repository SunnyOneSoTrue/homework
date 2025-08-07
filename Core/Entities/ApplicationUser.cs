using Microsoft.AspNetCore.Identity;

namespace homework.Core.Entities
{
    /// <summary>
    /// the user entity. esentially the base of whoever's trying to access the application.
    /// Inherits stuff from IdentityUser(email field, password n stuff)
    /// </summary>
    public class ApplicationUser : IdentityUser
    {
      
    }
}