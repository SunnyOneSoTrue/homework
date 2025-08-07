using Microsoft.AspNetCore.Identity;

namespace homework.Core.Entities
{
    /// <summary>
    /// the user entity. esentially the base of whoever's trying to access the application.
    /// Inherits stuff from IdentityUser(email field, password n stuff)
    /// custom stuff already added
    /// </summary>
    public class ApplicationUser : IdentityUser
    {
        private string fullName;
        public string FullName { get => fullName; set => fullName = value; }
        
        DateTime dateOfBirth;
        public DateTime DateOfBirth { get => dateOfBirth; set => dateOfBirth = value; }
        
        string roleDisplayName;
        public string RoleDisplayName { get => roleDisplayName; set => roleDisplayName = value; }
    }
}