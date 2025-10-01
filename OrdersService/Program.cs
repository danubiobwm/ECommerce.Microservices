using OrdersService.Data;
using OrdersService.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// --- Connection string com retry ---
var conn = builder.Configuration.GetConnectionString("Default")
           ?? builder.Configuration["ConnectionStrings__Default"]
           ?? $"Server={builder.Configuration["SQLSERVER_HOST"] ?? "mssql"},{builder.Configuration["SQLSERVER_PORT"] ?? "1433"};Database=OrdersDb;User Id={builder.Configuration["DB_USER"] ?? "sa"};Password={builder.Configuration["DB_PASSWORD"] ?? "Your_password123"};TrustServerCertificate=True;ConnectRetryCount=5;ConnectRetryInterval=10;MultipleActiveResultSets=true;";

// --- JWT config ---
var jwtSecret = builder.Configuration["JWT_SECRET"] ?? throw new Exception("JWT_SECRET não configurado");
var jwtIssuer = builder.Configuration["JWT_ISSUER"] ?? "ECommerce";
var jwtAudience = builder.Configuration["JWT_AUDIENCE"] ?? "ECommerceClients";
var key = Encoding.ASCII.GetBytes(jwtSecret);

// --- DbContext ---
builder.Services.AddDbContext<OrdersDbContext>(options => options.UseSqlServer(conn));

// --- Services ---
builder.Services.AddScoped<OrderService>();

// --- Auth ---
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = ctx =>
        {
            var logger = ctx.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogError(ctx.Exception, "JWT falhou em OrdersService");
            return Task.CompletedTask;
        },
        OnTokenValidated = ctx =>
        {
            var logger = ctx.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogInformation("JWT válido para {user}", ctx.Principal?.Identity?.Name);
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

// --- Swagger ---
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Orders API", Version = "v1" });
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

// --- Migrations auto ---
using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<OrdersDbContext>();
        db.Database.Migrate();
        logger.LogInformation("Migrations aplicadas em OrdersDb");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Erro aplicando migrations OrdersDb");
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
