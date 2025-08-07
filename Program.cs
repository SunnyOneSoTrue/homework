using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Infrastructure.Data;
using Core.Entities;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

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


builder.Services.AddAuthorization();

var app = builder.Build();

app.UseAuthentication(); //are you one of us?
app.UseAuthorization(); // what authority do you have?

app.MapControllers(); //maps controller methods. Lets app listen to specific urls

app.Run();//starts app ofc

