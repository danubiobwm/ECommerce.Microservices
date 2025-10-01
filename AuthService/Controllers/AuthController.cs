using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace AuthService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        // user list already in your code
        private static readonly List<(string Username, string Password, string Role)> Users = new() {
            ("admin","admin123","Admin"),
            ("user","user123","User")
        };

        private readonly IConfiguration _cfg;

        public AuthController(IConfiguration cfg) => _cfg = cfg;

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest req)
        {
            var user = Users.FirstOrDefault(u => u.Username == req.Username && u.Password == req.Password);
            if (user == default) return Unauthorized();

            var jwtSecret = _cfg["JWT_SECRET"] ?? throw new Exception("JWT_SECRET not configured");
            var jwtIssuer = _cfg["JWT_ISSUER"] ?? "ECommerce";
            var jwtAudience = _cfg["JWT_AUDIENCE"] ?? "ECommerceClients";
            var expiryMinutes = int.Parse(_cfg["JWT_EXPIRES_MINUTES"] ?? "60");

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim("role", user.Role),
                new Claim(JwtRegisteredClaimNames.Sub, user.Username),
            };
            if (!string.IsNullOrEmpty(user.Role))
            {
                claims.Add(new Claim(ClaimTypes.Role, user.Role));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: jwtIssuer,
                audience: jwtAudience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
                signingCredentials: creds
            );

            var tokenStr = new JwtSecurityTokenHandler().WriteToken(token);

            return Ok(new { token = tokenStr });
        }

        public class LoginRequest
        {
            public string Username { get; set; } = null!;
            public string Password { get; set; } = null!;
        }
    }
}
