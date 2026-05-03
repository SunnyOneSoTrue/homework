using System.ComponentModel.DataAnnotations;
using Client;

namespace homework.Shared.DTOs.Database;

public class UpdateClientDTO
{
    [Required, MaxLength(60)]
    public string FirstName { get; set; } = string.Empty;

    [Required, MaxLength(60)]
    public string LastName { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    public string? ProfilePhoto { get; set; }

    public List<Address> Addresses { get; set; } = [];
    public List<Account> Accounts { get; set; } = [];
}