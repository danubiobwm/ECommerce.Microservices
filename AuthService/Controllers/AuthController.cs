using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace AuthService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _config;
    public AuthController(IConfiguration config) { _config = config; }

    private static readonly List<(string Username, string Password, string Role)> Users = new() {
        ("admin","admin123","Admin"),
        ("user","user123","User")
    };

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginModel model)
    {
        var user = Users.FirstOrDefault(u => u.Username == model.Username && u.Password == model.Password);
        if (user == default) return Unauthorized(new { message = "Invalid credentials" });

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_config["JWT_SECRET"]);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role)
            }),
            Expires = DateTime.UtcNow.AddMinutes(int.Parse(_config["JWT_EXPIRES_MINUTES"] ?? "60")),
            Issuer = _config["JWT_ISSUER"],
            Audience = _config["JWT_AUDIENCE"],
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return Ok(new { token = tokenHandler.WriteToken(token) });
    }

    public class LoginModel { public string Username { get; set; } = ""; public string Password { get; set; } = ""; }
}
