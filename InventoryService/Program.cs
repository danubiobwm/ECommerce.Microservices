using InventoryService.Data;
using InventoryService.Services;
using InventoryService.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);


string connectionString = builder.Configuration["ConnectionStrings__Default"];
if (string.IsNullOrWhiteSpace(connectionString))
{
    var host = builder.Configuration["SQLSERVER_HOST"] ?? "localhost";
    var port = builder.Configuration["SQLSERVER_PORT"] ?? "1433";
    var dbName = builder.Configuration["INVENTORY_DB"] ?? "InventoryDb";
    var saPass = builder.Configuration["MSSQL_SA_PASSWORD"] ?? "P@ssw0rd123!";
    connectionString = $"Server={host},{port};Database={dbName};User Id=sa;Password={saPass};TrustServerCertificate=True;";
}

builder.Services.AddDbContext<InventoryDbContext>(opt => opt.UseSqlServer(connectionString));

builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddHostedService<OrderCreatedConsumer>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var jwtSecret = builder.Configuration["JWT_SECRET"] ?? throw new Exception("Missing JWT_SECRET");
var issuer = builder.Configuration["JWT_ISSUER"] ?? "ECommerce";
var audience = builder.Configuration["JWT_AUDIENCE"] ?? "ECommerceClients";
var key = Encoding.UTF8.GetBytes(jwtSecret);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = issuer,
            ValidateAudience = true,
            ValidAudience = audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateLifetime = true
        };
    });

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();
    var tried = 0;
    while (true)
    {
        try { db.Database.Migrate(); break; }
        catch
        {
            tried++;
            if (tried > 40) throw;
            await Task.Delay(5000);
        }
    }
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
