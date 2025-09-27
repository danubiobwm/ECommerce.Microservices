using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("ocelot.json");

// JWT
var key = Encoding.UTF8.GetBytes(builder.Configuration["JWT_SECRET"] ?? throw new Exception("Missing JWT_SECRET"));
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer("TestKey", options =>
    {
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["JWT_ISSUER"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["JWT_AUDIENCE"],
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key)
        };
    });

builder.Services.AddOcelot(builder.Configuration);
var app = builder.Build();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

await app.UseOcelot();

app.Run();
