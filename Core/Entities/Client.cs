using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;

namespace Client
{
    public class Client : IValidatableObject
    {
        public int Id { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; } = default!;

        [Required, MaxLength(60)]
        public string FirstName { get; set; } = default!;

        [Required, MaxLength(60)]
        public string LastName { get; set; } = default!;

        [Required, RegularExpression(@"^\+[1-9]\d{7,14}$", ErrorMessage = "Phone must be E.164 format, e.g. +15551234567.")]
        public string MobileNumber { get; set; } = default!;

        [Required, StringLength(11, MinimumLength = 11)]
        public string PersonalId { get; set; } = default!;   // string(11), NOT a key

        [Required]
        public Sex Sex { get; set; }

        [Required]
        public List<Account> Accounts { get; set; } = new();

        [Required]
        public List<Address> Addresses { get; set; } = new();

        public string? ProfilePhoto { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext _)
        {
            if (Accounts.Count == 0)
                yield return new ValidationResult("At least one account is required.", new[] { nameof(Accounts) });

            if (Addresses.Count == 0)
                yield return new ValidationResult("At least one address is required.", new[] { nameof(Addresses) });
        }
    }

    public class Address
    {
        public int Id { get; set; } 
        public int ClientId { get; set; }     

        [JsonIgnore]
        public Client? Client { get; set; }
        [Required] public string Country { get; set; } = default!;
        [Required] public string City { get; set; } = default!;
        [Required] public string Street { get; set; } = default!;
        [Required] public string ZipCode { get; set; } = default!;
    }

    public class Account
    {
        public int Id { get; set; }  
        public int ClientId { get; set; } 
        [JsonIgnore]
        public Client? Client { get; set; }

        [Required] public string AccountNumber { get; set; } = default!;
        public decimal Balance { get; set; }
    }

    public enum Sex { Male, Female }
}

