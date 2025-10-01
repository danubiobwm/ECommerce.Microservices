using InventoryService.Data;
using InventoryService.Services;
using InventoryService.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

var conn = builder.Configuration.GetConnectionString("Default")
           ?? builder.Configuration["ConnectionStrings__Default"]
           ?? $"Server={builder.Configuration["SQLSERVER_HOST"] ?? "mssql"},{builder.Configuration["SQLSERVER_PORT"] ?? "1433"};Database={builder.Configuration["INVENTORY_DB"] ?? "InventoryDb"};User Id={builder.Configuration["DB_USER"] ?? "sa"};Password={builder.Configuration["MSSQL_SA_PASSWORD"] ?? builder.Configuration["MSSQL_SA_PASSWORD"] ?? builder.Configuration["MSSQL_SA_PASSWORD"] ?? "P@ssw0rd123!"};TrustServerCertificate=True;ConnectRetryCount=5;ConnectRetryInterval=10;MultipleActiveResultSets=true;";

var jwtSecret = builder.Configuration["JWT_SECRET"] ?? throw new Exception("JWT_SECRET not provided");
var jwtIssuer = builder.Configuration["JWT_ISSUER"] ?? builder.Configuration["JWT:Issuer"] ?? "ECommerce";
var jwtAudience = builder.Configuration["JWT_AUDIENCE"] ?? builder.Configuration["JWT:Audience"] ?? "ECommerceClients";
var key = Encoding.ASCII.GetBytes(jwtSecret);

builder.Services.AddDbContext<InventoryDbContext>(options => options.UseSqlServer(conn));
builder.Services.AddScoped<ProductService>();

builder.Services.AddHostedService<OrderCreatedConsumer>();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.Events = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents
    {
        OnAuthenticationFailed = ctx =>
        {
            var logger = ctx.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogError(ctx.Exception, "Authentication failed in InventoryService");
            return Task.CompletedTask;
        },
        OnTokenValidated = ctx =>
        {
            var logger = ctx.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogInformation("Token validated for {user}", ctx.Principal?.Identity?.Name);
            return Task.CompletedTask;
        }
    };
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = jwtIssuer,
        ValidateAudience = true,
        ValidAudience = jwtAudience,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Inventory API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Use: 'Bearer {token}'"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement {
        { new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer"}}, Array.Empty<string>() }
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();
        db.Database.Migrate();
        logger.LogInformation("Applied Inventory migrations");
    }
    catch (Exception ex)
    {
        var logger2 = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger2.LogError(ex, "Failed to apply migrations for InventoryDb");
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}



app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();
