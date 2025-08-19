using System.Security.Claims;
using System.Text.Json;
using Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using homework.Controllers.SearchParams;

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
    public async Task<IActionResult> Update(int id, [FromBody] Client.Client client)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        var fetched = await _db.Clients
            .Include(c => c.Addresses)
            .Include(c => c.Accounts)
            .FirstOrDefaultAsync(c => c.Id == id);
        if (fetched is null) return NotFound();

        _db.Addresses.RemoveRange(fetched.Addresses);
        _db.Accounts.RemoveRange(fetched.Accounts);
        fetched.Addresses = client.Addresses;
        fetched.Accounts  = client.Accounts;

        await _db.SaveChangesAsync();
        return NoContent();
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