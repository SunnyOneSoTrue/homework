using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Options;

namespace Client
{
    public class Client: IValidatableObject
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        
        [Required, MaxLength(60)]
        public string FirstName { get; set; }
        
        [Required, MaxLength(60)]
        public string LastName { get; set; }
        
        [Required]
        [RegularExpression(@"^\+[1-9]\d{7,14}$", ErrorMessage = "Phone must be E.164 format, e.g. +15551234567.")]
        public string MobileNumber { get; set; }
        
        [StringLength(11, MinimumLength = 11)]
        public int Id { get; set; }

        public int PersonalId { get; set; }

        [Required]
        public Sex Sex { get; set; }
        
        public List<Account> Accounts { get; set; } = new();

        [Required] public List<Address> Addresses { get; set; } = new();
        
        public string ProfilePhoto { get; set; }
        
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Accounts.Count == 0)
            {
                yield return new ValidationResult("At least one account is required.",new[] { nameof(Accounts) });
            }
        }
    }

    public class Address
    {
        public int Id { get; set; }            
        public int ClientId { get; set; }
        public Client Client { get; set; }
        [Required] public string Country { get; set; }
        [Required] public string City { get; set; }
        [Required] public string Street { get; set; }
        [Required] public string ZipCode { get; set; }
    }

    public class Account
    {
        public int Id { get; set; }
        public int ClientId { get; set; }    
        public Client Client { get; set; }
    }
    
    public enum Sex { Male, Female }
}

