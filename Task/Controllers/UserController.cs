// Controllers/AuthController.cs
using Microsoft.AspNetCore.Mvc;
using Task.Data;
using Task.Models;
using Task.DTOs;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using System.Linq;

namespace Task.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // POST: api/auth/register
        [HttpPost("register")]
        public IActionResult Register([FromBody] UserRegisterDTO userRegister)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Check if user already exists
            if (_context.Users.Any(u => u.Username.ToLower() == userRegister.Username.ToLower()))
                return BadRequest(new { Message = "User already exists." });

            var user = new User
            {
                Username = userRegister.Username,
                Password = userRegister.Password, // **Note:** Password stored as plain text. Not recommended.
                Role = "User" // Default role
            };
            _context.Users.Add(user);
            _context.SaveChanges();

            return Ok(new { Message = "User registered successfully." });
        }

        // POST: api/auth/login
        [HttpPost("login")]
        public IActionResult Login([FromBody] UserLoginDTO loginUser)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = _context.Users.SingleOrDefault(u => u.Username.ToLower() == loginUser.Username.ToLower() && u.Password == loginUser.Password);
            if (user == null)
                return Unauthorized(new { Message = "Invalid credentials." });

            var token = GenerateJwtToken(user);
            return Ok(new { Token = token, Message = "Login successful." });
        }

        private string GenerateJwtToken(User user)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:SecretKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
