using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

var jwtSecret = builder.Configuration["JWT_SECRET"] ?? throw new Exception("JWT_SECRET not set");
var jwtIssuer = builder.Configuration["JWT_ISSUER"] ?? "ECommerce";
var jwtAudience = builder.Configuration["JWT_AUDIENCE"] ?? "ECommerceClients";
var key = Encoding.ASCII.GetBytes(jwtSecret);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
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

builder.Services.AddAuthorization();
builder.Services.AddHttpClient();

var app = builder.Build();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.Map("/{**catchall}", async (HttpContext ctx, IHttpClientFactory httpFactory) =>
{
    // simply proxy requests by path prefix:
    var path = ctx.Request.Path.ToString();
    var targetBase = "";
    if (path.StartsWith("/api/Products") || path.StartsWith("/products") || path.StartsWith("/inventory"))
        targetBase = builder.Configuration["INVENTORY_URL"] ?? "http://inventory:5002";
    else if (path.StartsWith("/api/Orders") || path.StartsWith("/orders"))
        targetBase = builder.Configuration["ORDERS_URL"] ?? "http://orders:5003";
    else
    {
        ctx.Response.StatusCode = 502;
        await ctx.Response.WriteAsync("Unknown route");
        return;
    }

    // Build forwarded URL
    var forwardUrl = $"{targetBase}{ctx.Request.Path}{ctx.Request.QueryString}";
    var client = httpFactory.CreateClient();
    var req = new HttpRequestMessage(new HttpMethod(ctx.Request.Method), forwardUrl);

    // copy headers (including Authorization)
    foreach (var h in ctx.Request.Headers)
    {
        // skip host
        if (!string.Equals(h.Key, "Host", StringComparison.OrdinalIgnoreCase))
            req.Headers.TryAddWithoutValidation(h.Key, (IEnumerable<string>)h.Value);
    }

    // copy body if present
    if (ctx.Request.ContentLength > 0)
    {
        req.Content = new StreamContent(ctx.Request.Body);
        if (ctx.Request.ContentType != null)
            req.Content.Headers.TryAddWithoutValidation("Content-Type", ctx.Request.ContentType);
    }

    var resp = await client.SendAsync(req);
    ctx.Response.StatusCode = (int)resp.StatusCode;
    foreach (var h in resp.Headers)
        ctx.Response.Headers[h.Key] = h.Value.ToArray();
    foreach (var h in resp.Content.Headers)
        ctx.Response.Headers[h.Key] = h.Value.ToArray();

    // remove transfer-encoding if set
    ctx.Response.Headers.Remove("transfer-encoding");

    await resp.Content.CopyToAsync(ctx.Response.Body);
});

app.Run();
