using Microsoft.AspNetCore.Mvc;
using InternassignmentBackend.Models;
using System.Linq;


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
using System.Text;

namespace InternassignmentBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoleController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;

        public RoleController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpPost("create")]
        public IActionResult CreateRole([FromBody] Role role)
        {
            if (string.IsNullOrEmpty(role.RoleName))
            {
                return BadRequest("Role name cannot be empty.");
            }

            if (_dbContext.Roles.Any(r => r.RoleName == role.RoleName))
            {
                return Conflict("Role already exists.");
            }

            _dbContext.Roles.Add(role);
            _dbContext.SaveChanges();
            return Ok(new { Message = "Role created successfully", Role = role });
        }

        [HttpGet("all")]
        public IActionResult GetAllRoles()
        {
            var roles = _dbContext.Roles.ToList();
            return Ok(roles);
        }

        [HttpDelete("{roleId}")]
        public IActionResult DeleteRole(int roleId)
        {
            var role = _dbContext.Roles.FirstOrDefault(r => r.RoleId == roleId);
            if (role == null)
            {
                return NotFound("Role not found.");
            }

            _dbContext.Roles.Remove(role);
            _dbContext.SaveChanges();
            return Ok(new { Message = "Role deleted successfully" });
        }
    }
}
