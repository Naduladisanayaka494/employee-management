using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using InternassignmentBackend.Data;
using InternassignmentBackend.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
namespace InternassignmentBackend.Controllers
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

        [HttpPost("admin/login")]
        public IActionResult AdminLogin([FromBody] LoginDto login)
        {
          
            var admin = _context.Employees.FirstOrDefault(e => e.Email == login.Email && e.RoleId == 1);

            if (admin == null)
                return Unauthorized("Invalid Admin credentials");

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, admin.Name),
                new Claim(ClaimTypes.NameIdentifier, admin.Id.ToString()),
                new Claim(ClaimTypes.Role, "Admin")
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: creds
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
            return Ok(new { Token = tokenString });
        }


        [HttpPost("employee/login")]
        public IActionResult EmployeeLogin([FromBody] LoginDto login)
        {
          
            var employee = _context.Employees.FirstOrDefault(e => e.Email == login.Email);

            if (employee == null)
                return Unauthorized("Invalid Employee credentials");

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, employee.Name),
                new Claim(ClaimTypes.NameIdentifier, employee.Id.ToString()),
                new Claim(ClaimTypes.Role, "Employee")
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: creds
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
            return Ok(new { Token = tokenString });
        }


        [HttpPost("admin/create")]
    
        public async Task<IActionResult> CreateAdmin([FromBody] SignupDto signupDto)
        {
      
            var existingAdmin = await _context.Employees
                .FirstOrDefaultAsync(e => e.Email == signupDto.Email && e.RoleId == 1); // Check if Admin already exists

            if (existingAdmin != null)
            {
                return BadRequest("An Admin with this email already exists.");
            }

       
            var hashedPassword = HashPassword(signupDto.Password);

        
            var admin = new Employee
            {
                Name = signupDto.Name,
                Email = signupDto.Email,
                Password = hashedPassword,
                RoleId = 1 
            };

            _context.Employees.Add(admin);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Admin created successfully." });
        }

        [HttpPost("employee/create")]
        [Authorize(Roles = "Admin")] 
        public async Task<IActionResult> CreateEmployee([FromBody] SignupDto signupDto)
        {
          
            var existingEmployee = await _context.Employees
                .FirstOrDefaultAsync(e => e.Email == signupDto.Email);

            if (existingEmployee != null)
            {
                return BadRequest("An employee with this email already exists.");
            }

   
            var hashedPassword = HashPassword(signupDto.Password);

   
            var employee = new Employee
            {
                Name = signupDto.Name,
                Email = signupDto.Email,
                Password = hashedPassword,
                RoleId = 2 
            };

            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Employee created successfully." });
        }

   
        private string HashPassword(string password)
        {
            var salt = new byte[16];
            using (var rng = new System.Security.Cryptography.RNGCryptoServiceProvider())
            {
                rng.GetBytes(salt);
            }

            var hashedPassword = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 10000,
                numBytesRequested: 256 / 8
            ));

            return hashedPassword;
        }
    }
}
