using FinalProject.BL.Services;
using FinalProject.DAL.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace FinalProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AuthenticationService _authService;
        private readonly AuditTrailService _auditTrailService;
        private readonly IConfiguration _configuration;

        public AuthController(IConfiguration configuration)
        {
            _authService = new AuthenticationService(configuration);
            _auditTrailService = new AuditTrailService(configuration);
            _configuration = configuration;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            try
            {
                if (string.IsNullOrEmpty(model.Username) || string.IsNullOrEmpty(model.Password))
                    return BadRequest("Username and password are required");

                var person = _authService.Authenticate(model.Username, model.Password);
                if (person == null)
                    return Unauthorized("Invalid username or password");

                // יצירת טוקן JWT
                var token = GenerateJwtToken(person);

                // לוג הפעולה
                await _auditTrailService.LogActionAsync(
                    person.PersonId,
                    "Login",
                    "Auth",
                    0,
                    $"User logged in: {person.Username}"
                );

                return Ok(new { Token = token, Person = person });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            try
            {
                var currentUserId = User.Identity.Name;

                // לוג הפעולה
                await _auditTrailService.LogActionAsync(
                    currentUserId,
                    "Logout",
                    "Auth",
                    0,
                    $"User logged out"
                );

                // הערה: JWT הוא stateless - אין באמת דרך לבטל אותו עד שהוא פג תוקף
                // באפליקציה אמיתית, אפשר להשתמש ברשימת שלילה של טוקנים או Redis לפתרון מלא

                return Ok(new { Message = "Logout successful" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        private string GenerateJwtToken(Person person)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            // קבלת התפקידים של המשתמש
            var roleService = new RoleService(_configuration);
            var roles = roleService.GetPersonRoles(person.PersonId);

            // יצירת רשימת ה-claims
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, person.PersonId),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Name, person.PersonId),
                new Claim("username", person.Username)
            };

            // הוספת התפקידים כ-claims
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role.RoleName));
            }

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(2),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }

    public class LoginModel
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}