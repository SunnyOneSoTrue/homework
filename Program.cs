using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Infrastructure.Data;
using Core.Entities;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json", optional: false, reloadOnChange: true); //I add the appsettings.json file manually since I created the file manually also.


//initially adding the services that we will be using
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite("Data Source=banking.db"));
builder.Services.AddIdentity<ApplicationUser, IdentityRole>().AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();

var jwtKey = builder.Configuration["Jwt:Key"] ?? throw new Exception("no JWT key"); //this gets the key that is used to stamp tokens
var keyBytes = Encoding.UTF8.GetBytes(jwtKey); //this turns the key into bytes so that the signing algorythm can read it 


builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme =
        JwtBearerDefaults.AuthenticationScheme; //"Use JWT Bearer tokens for authentication"
    options.DefaultChallengeScheme =
        JwtBearerDefaults.AuthenticationScheme; //"if it fails, tell the user the default way"
}).AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false; // allows local dev using http
    options.SaveToken = true; //Stores the token in the current request context 
    options.TokenValidationParameters = new TokenValidationParameters {
        ValidateIssuerSigningKey = true, //Ensures the token has a valid digital signature
        IssuerSigningKey = new SymmetricSecurityKey(keyBytes), //the secret key used to verify the signature.
        ValidateIssuer = false, // doesn't check where it comes from
        ValidateAudience = false // doesn't check for whom it was intended for
    };

});

builder.Services.Configure<SeedRolesSettings.SeedRolesSettings>(builder.Configuration.GetSection("SeedRoles")); //to add seedRoles class
builder.Services.Configure<SeedAdminSettings.SeedAdminSettings>(builder.Configuration.GetSection("SeedAdmin")); //to add the base admin seed

builder.Services.AddAuthorization(); //adds the authorisation
builder.Services.AddControllers(); // adds the controllers that we define

var app = builder.Build();

using (var scope = app.Services.CreateScope()) //code for role seeding. Checks if role and admin exists and creates it if it doesn't 
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>(); // used to create roles if not existing
    var roleSettings = app.Services.GetRequiredService<IOptions<SeedRolesSettings.SeedRolesSettings>>().Value;
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>(); //used to assign admin to initial admin role in appsettings.json
    var adminSettings = app.Services.GetRequiredService<IOptions<SeedAdminSettings.SeedAdminSettings>>().Value;

    foreach (var roleName in roleSettings.Roles)//loops though an creates roles if they dont already exist
    {
        var exists = await roleManager.RoleExistsAsync(roleName);
        if (!exists)
        {
            await roleManager.CreateAsync(new IdentityRole(roleName)); 
        }
    }
    var adminUser = await userManager.FindByEmailAsync(adminSettings.Email);//assigns admin role to initial values if it doesnt already exist
    if (adminUser == null)
    {
        adminUser = new IdentityUser
        {
            UserName = adminSettings.Email,
            Email = adminSettings.Email,
            EmailConfirmed = true
        };
        var result = await userManager.CreateAsync(adminUser, adminSettings.Password);
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, adminSettings.Role);
        }
}

app.UseAuthentication(); //are you one of us?
app.UseAuthorization(); // what authority do you have?

app.MapControllers(); //maps controller methods. Lets app listen to specific urls

app.Run();//starts app ofc

