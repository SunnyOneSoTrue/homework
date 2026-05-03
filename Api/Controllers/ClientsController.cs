using System.Security.Claims;
using System.Text.Json;
using Client;
using Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using homework.Controllers.SearchParams;
using homework.Shared.DTOs.Database;

namespace homework.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClientsController: ControllerBase
{
    private readonly ApplicationDbContext _db;
    public ClientsController(ApplicationDbContext db) => _db = db;

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateClientDTO request)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        var client = new Client.Client
        {
            FirstName    = request.FirstName,
            LastName     = request.LastName,
            Email        = request.Email,
            PersonalId   = request.PersonalId,
            ProfilePhoto = request.ProfilePhoto,
            Addresses    = request.Addresses.Select(a => new Address
            {
                Country = a.Country,
                City    = a.City,
                Street  = a.Street,
                ZipCode = a.ZipCode
            }).ToList(),
            Accounts = request.Accounts.Select(a => new Account
            {
                AccountNumber = a.AccountNumber
            }).ToList()
        };
        
        _db.Clients.Add(client);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = client.Id }, new { client.Id });
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("search-suggestions")]
    public async Task<IActionResult> GetSuggestions()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId)) return Forbid();

        var jsonList = await _db.SearchHistories
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAtUtc)
            .Take(3)
            .Select(x => x.QueryJson!)
            .ToListAsync();

        var suggestions = jsonList
            .Select(j => JsonSerializer.Deserialize<SearchParams.SearchParams>(j))
            .Where(x => x != null)!
            .ToList();

        return Ok(suggestions);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("all-clients")]
    public async Task<IActionResult> GetAllClients()
    {
        var clients = await _db.Clients
            .Include(c => c.Addresses)
            .Include(c => c.Accounts)
            .ToListAsync();

        return Ok(clients);
    }


    [Authorize]
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var client = await _db.Clients
            .AsNoTracking()
            .Include(c => c.Addresses)
            .Include(c => c.Accounts)
            .FirstOrDefaultAsync(c => c.Id == id);

        return client is null ? NotFound() : Ok(client);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateClientDTO request)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        var fetched = await _db.Clients
            .Include(c => c.Addresses)
            .Include(c => c.Accounts)
            .FirstOrDefaultAsync(c => c.Id == id);
        if (fetched is null) return NotFound();

        fetched.FirstName    = request.FirstName;
        fetched.LastName     = request.LastName;
        fetched.Email        = request.Email;
        fetched.ProfilePhoto = request.ProfilePhoto;
        
        _db.Addresses.RemoveRange(fetched.Addresses);
        _db.Accounts.RemoveRange(fetched.Accounts);
        
        fetched.Addresses = request.Addresses.Select(a => new Address
        {
            Country   = a.Country,
            City      = a.City,
            Street    = a.Street,
            ZipCode   = a.ZipCode,
            ClientId  = id
        }).ToList();
        
        fetched.Accounts = request.Accounts.Select(a => new Account
        {
            AccountNumber = a.AccountNumber,
            ClientId      = id
        }).ToList();

        await _db.SaveChangesAsync();
        return NoContent();
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var client = await _db.Clients.FirstOrDefaultAsync(c => c.Id == id);
        _db.Remove(client);
        await _db.SaveChangesAsync();
        return Ok("Client deleted");
    }

}