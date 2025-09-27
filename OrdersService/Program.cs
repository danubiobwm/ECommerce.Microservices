using OrdersService.Data;
using OrdersService.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Build connection string similar to Inventory
var connRaw = builder.Configuration["ConnectionStrings__Default"] ?? $"{builder.Configuration["SQLSERVER_HOST"]},1433,{builder.Configuration["MSSQL_SA_PASSWORD"]},{builder.Configuration["ORDERS_DB"]}";
var parts = connRaw.Split(',');
var host = parts[0]; var port = parts.Length > 1 ? parts[1] : "1433"; var saPass = parts.Length > 2 ? parts[2] : builder.Configuration["MSSQL_SA_PASSWORD"]; var dbName = parts.Length > 3 ? parts[3] : builder.Configuration["ORDERS_DB"] ?? "OrdersDb";
var connectionString = $"Server={host},{port};Database={dbName};User Id=sa;Password={saPass};TrustServerCertificate=True;";

builder.Services.AddDbContext<OrdersDbContext>(opt => opt.UseSqlServer(connectionString));
builder.Services.AddScoped<OrderService>();
builder.Services.AddHttpClient("inventory", c => {
    c.BaseAddress = new Uri(builder.Configuration["INVENTORY_URL"] ?? "http://inventory:5002");
    c.Timeout = TimeSpan.FromSeconds(10);
});

builder.Services.AddControllers();
builder.Services.AddSwaggerGen();

// JWT
var key = Encoding.UTF8.GetBytes(builder.Configuration["JWT_SECRET"] ?? throw new Exception("Missing JWT_SECRET"));
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
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

var app = builder.Build();

// Apply migrations with retry
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<OrdersDbContext>();
    var tried = 0;
    while (true)
    {
        try
        {
            db.Database.Migrate();
            break;
        }
        catch
        {
            tried++;
            if (tried > 20) throw;
            await Task.Delay(3000);
        }
    }
}

if (app.Environment.IsDevelopment()) { app.UseSwagger(); app.UseSwaggerUI(); }

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
