using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Npgsql;

namespace LynxPay.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _config;

        public AuthController(IConfiguration config)
        {
            _config = config;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            var connStr = _config.GetConnectionString("CentralDb")
                ?? throw new InvalidOperationException("CentralDb connection not configured");

            await using var conn = new NpgsqlConnection(connStr);
            await conn.OpenAsync();

            var cmd = new NpgsqlCommand(@"
                SELECT u.password_hash, c.database_name
                FROM users u
                JOIN clients c ON c.id = u.client_id
                WHERE u.email = @e", conn);

            cmd.Parameters.AddWithValue("e", request.Email);

            await using var reader = await cmd.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
                return Unauthorized();

            if (reader.GetString(0) != request.Password)
                return Unauthorized();

            var dbName = reader.GetString(1);

            var claims = new[]
            {
                new Claim("db", dbName),
                new Claim(ClaimTypes.Email, request.Email)
            };

            var jwtKey = _config["Jwt:Key"]
                ?? throw new InvalidOperationException("JWT Key not configured");

            var jwtIssuer = _config["Jwt:Issuer"]
                ?? throw new InvalidOperationException("JWT Issuer not configured");

            var jwtAudience = _config["Jwt:Audience"]
                ?? throw new InvalidOperationException("JWT Audience not configured");

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtKey));

            var token = new JwtSecurityToken(
                issuer: jwtIssuer,
                audience: jwtAudience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(60),
                signingCredentials:
                    new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
            );

            return Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token)
            });
        }
    }

    public record LoginRequest(string Email, string Password);
}
