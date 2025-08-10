using System.Security.Claims;
using System.Text.Json;
using Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace homework.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClientsController: ControllerBase
{
    private readonly ApplicationDbContext _db;
    public ClientsController(ApplicationDbContext db) => _db = db;

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Client.Client client)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        
        _db.Clients.Add(client);
        await _db.SaveChangesAsync();
        
        return CreatedAtAction(nameof(GetById), new { id = client.Id }, new { client.Id });
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("search-suggestions")]
    public async Task<ActionResult> GetSuggestions(int id)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId)) return Forbid();

        var jsonList = await _db.SearchHistories
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAtUtc)
            .Take(3)
            .Select(x => x.QueryJson)
            .ToListAsync();

        // define a small shape for suggestions
        var suggestions = jsonList
            .Select(j => JsonSerializer.Deserialize<SearchParams>(j))
            .Where(x => x != null)
            .ToList();

        return Ok(suggestions);
    }

    [Authorize]
    [HttpGet("{id:int}")]
    public async Task<Client.Client> GetById(int id)
    {
        var client = await _db.Clients
            .AsNoTracking()
            .Include(c => c.Addresses)
            .Include(c => c.Accounts)
            .FirstOrDefaultAsync(c => c.Id == id);
        if (client == null) return null;
        return client; 

    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] Client.Client client)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        
        var fetchedClient = await _db.Clients.Include(c => c.Addresses).Include(c => c.Accounts).FirstOrDefaultAsync(c => c.Id == id);
        if (fetchedClient == null) return NotFound();
        
        _db.Addresses.RemoveRange(fetchedClient.Addresses);
        _db.Accounts.RemoveRange(fetchedClient.Accounts);
        fetchedClient.Addresses = client.Addresses; 
        fetchedClient.Accounts  = client.Accounts;

        _db.SaveChanges();
        return Ok("saved");
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var client = await _db.Clients.FirstOrDefaultAsync(c => c.Id == id);
        _db.Remove(client);
        _db.SaveChanges();
        return Ok("Client deleted");
    }

}

public record SearchParams(int page, int pageSize, string? search, string? sortBy, string? sortDir);