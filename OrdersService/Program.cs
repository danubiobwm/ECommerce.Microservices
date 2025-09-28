using OrdersService.Data;
using OrdersService.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using RabbitMQ.Client;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Connection string
var connectionString = builder.Configuration.GetConnectionString("Default") ??
                       builder.Configuration["ConnectionStrings__Default"] ??
                       throw new Exception("Missing ConnectionStrings:Default");

// JWT
var jwtSecret = builder.Configuration["JWT_SECRET"] ?? throw new Exception("Missing JWT_SECRET");
var jwtIssuer = builder.Configuration["JWT_ISSUER"] ?? "ECommerce";
var jwtAudience = builder.Configuration["JWT_AUDIENCE"] ?? "ECommerceAudience";
var key = Encoding.UTF8.GetBytes(jwtSecret);

// DbContext
builder.Services.AddDbContext<OrdersDbContext>(options =>
    options.UseSqlServer(connectionString, sql =>
    {
        sql.MigrationsAssembly("OrdersService");
    }));

// Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,
            ValidateAudience = true,
            ValidAudience = jwtAudience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateLifetime = true
        };
    });

builder.Services.AddControllers();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "OrdersService", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Use: Bearer {token}",
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement {
        {
            new OpenApiSecurityScheme {
                Reference = new OpenApiReference {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[]{}
        }
    });
});

// RabbitMQ connection factory registration
builder.Services.AddSingleton(sp =>
{
    var factory = new ConnectionFactory()
    {
        HostName = builder.Configuration["RABBITMQ_HOST"] ?? "localhost",
        UserName = builder.Configuration["RABBITMQ_USER"] ?? "guest",
        Password = builder.Configuration["RABBITMQ_PASS"] ?? "guest",
        DispatchConsumersAsync = true
    };
    return factory;
});

// Registrar serviços de domínio: (ajuste conforme seus nomes)
// builder.Services.AddScoped<IOrderService, OrderService>();
// builder.Services.AddScoped<IPublishService, RabbitMqPublishService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var env = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();
    if (env.IsDevelopment())
    {
        var db = scope.ServiceProvider.GetRequiredService<OrdersDbContext>();
        db.Database.Migrate();
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
