using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Core.Entities;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using RegisterRequest = homework.DTOs.Auth.RegisterRequest;

namespace homework.Controllers
{
    [ApiController]
    [Route("api/[controller]")] // actual url: "api/auth"
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _config;
        private SymmetricSecurityKey _key;

        public AuthController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager,  IConfiguration config)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _config = config;
            GenerateKey();
        }

        private void GenerateKey()
        {
            var rawKey = _config["Jwt:Key"] ?? throw new Exception("JWT key missing");
            _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(rawKey));
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            var user  = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                return BadRequest("user not found");
            }
            else if (!await _userManager.CheckPasswordAsync(user, request.Password))
            {
                return BadRequest("invalid password");
            }
            
           
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),        // who is this - unique id
                new Claim(ClaimTypes.Email, user.Email ?? string.Empty), //email address
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()) // unique token id
            };
            var roles = await _userManager.GetRolesAsync(user);
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role)); //assigns all roles that user is authorised to
            }

            var creds = new SigningCredentials(_key, SecurityAlgorithms.HmacSha256); //creates singingKey from jwt key. HmacSha256 is like a stamp
            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: creds
            );
            
            var jwt = new JwtSecurityTokenHandler().WriteToken(token); // creates token
            return Ok(new { token = jwt, expiresAt = token.ValidTo });
           
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequest request) //checks if role is correct, isn't already registered, registers and assigns role
        {
            if (!await _roleManager.RoleExistsAsync(request.Role))
                return BadRequest("role unrecognised");
            if (await _userManager.FindByEmailAsync(request.Email) != null)
            {
                return BadRequest("user exists");
            }

            var user = new ApplicationUser
            {
                UserName = request.Email,
                Email = request.Email
            };

            var result = await _userManager.CreateAsync(user, request.Password);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            await _userManager.AddToRoleAsync(user, request.Role);

            return Ok("User registered successfully");
        }
    }
}