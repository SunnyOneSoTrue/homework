using System.Security.Claims;
using System.Text.Json;
using Client;
using homework.Core.Entities;
using homework.Controllers.SearchParams;
using homework.Shared.DTOs.Database;
using homework.Infrastructure.UnitOfWork;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace homework.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClientsController : ControllerBase
{
    private readonly IUnitOfWork _uow;
    public ClientsController(IUnitOfWork uow) => _uow = uow;

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
                Country = a.Country, City = a.City, Street = a.Street, ZipCode = a.ZipCode
            }).ToList(),
            Accounts = request.Accounts.Select(a => new Account
            {
                AccountNumber = a.AccountNumber
            }).ToList()
        };

        await _uow.Clients.AddAsync(client);
        await _uow.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = client.Id }, new { client.Id });
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("search-suggestions")]
    public async Task<IActionResult> GetSuggestions()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId)) return Forbid();

        var jsonList = await _uow.Clients.GetSearchHistoryJsonAsync(userId);

        var suggestions = jsonList
            .Select(j => JsonSerializer.Deserialize<SearchParams.SearchParams>(j))
            .Where(x => x != null)!
            .ToList();

        return Ok(suggestions);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] SearchParams.SearchParams searchParams)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId)) return Forbid();

        var (total, results) = await _uow.Clients.SearchAsync(searchParams);

        await _uow.Clients.AddSearchHistoryAsync(userId, JsonSerializer.Serialize(searchParams));
        await _uow.SaveChangesAsync();

        return Ok(new { total, results });
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("all-clients")]
    public async Task<IActionResult> GetAllClients()
    {
        var clients = await _uow.Clients.GetAllAsync();
        return Ok(clients);
    }

    [Authorize]
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var client = await _uow.Clients.GetByIdAsync(id);
        return client is null ? NotFound() : Ok(client);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateClientDTO request)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        var fetched = await _uow.Clients.GetByIdAsync(id);
        if (fetched is null) return NotFound();

        fetched.FirstName    = request.FirstName;
        fetched.LastName     = request.LastName;
        fetched.Email        = request.Email;
        fetched.ProfilePhoto = request.ProfilePhoto;

        _uow.Clients.RemoveAddresses(fetched.Addresses);
        _uow.Clients.RemoveAccounts(fetched.Accounts);

        fetched.Addresses = request.Addresses.Select(a => new Address
        {
            Country = a.Country, City = a.City, Street = a.Street, ZipCode = a.ZipCode, ClientId = id
        }).ToList();

        fetched.Accounts = request.Accounts.Select(a => new Account
        {
            AccountNumber = a.AccountNumber, ClientId = id
        }).ToList();

        await _uow.SaveChangesAsync();
        return NoContent();
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var client = await _uow.Clients.GetByIdAsync(id);
        if (client is null) return NotFound();

        _uow.Clients.Remove(client);
        await _uow.SaveChangesAsync();
        return Ok("Client deleted");
    }
}