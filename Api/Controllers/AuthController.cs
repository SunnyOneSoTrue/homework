using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using homework.DTOs.Auth;

namespace homework.Controllers
{
    [ApiController]
    [Route("api/[controller]")] // actual url: "api/auth"
    public class AuthController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AuthController(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
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

            var user = new IdentityUser
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