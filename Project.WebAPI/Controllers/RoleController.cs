using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Project.Business.Model;
using Project.Data.Entity;
using System.Security.Claims;

namespace Project.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class RoleController : ControllerBase
    {
        private readonly RoleManager<Role> _roleManager;

        public RoleController(RoleManager<Role> roleManager)
        {
            _roleManager = roleManager;
        }

        
        [HttpGet]
        [Authorize(Roles = "Admin")]

        public async Task<IActionResult> GetRoles()
        {
            var roles = _roleManager.Roles.ToList();
            var roleViewModels = roles.Select(role => new RoleViewModel
            {
                Id = role.Id,
                Name = role.Name,
                Description = role.Description,
                IsActive = role.IsActive
            }).ToList();

            return Ok(roleViewModels);
        }

        // GET: api/Role/{id}
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]

        public async Task<IActionResult> GetRole(Guid id)
        {
            var role = await _roleManager.FindByIdAsync(id.ToString());
            if (role == null) return NotFound();

            var roleViewModel = new RoleViewModel
            {
                Id = role.Id,
                Name = role.Name,
                Description = role.Description,
                IsActive = role.IsActive
            };

            return Ok(roleViewModel);
        }

        // POST: api/Role
        [HttpPost]
        [Authorize(Roles = "Admin")]

        public async Task<IActionResult> CreateRole([FromBody] RoleViewModel model)
        {
            if (model == null || string.IsNullOrEmpty(model.Name))
            {
                return BadRequest("Role name cannot be empty");
            }

            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdString))
            {
                return Unauthorized("User ID not found in claims.");
            }

            if (!Guid.TryParse(userIdString, out var userId))
            {
                return BadRequest("Invalid user ID format.");
            }

            var role = new Role
            {
                Id = Guid.NewGuid(),
                Name = model.Name,
                Description = model.Description,
                IsActive = model.IsActive,
                CreatedAt = DateTime.Now, // Set CreatedAt
                CreatedBy = userId // Set CreatedBy as Guid
            };

            var result = await _roleManager.CreateAsync(role);

            if (result.Succeeded)
            {
                return Ok(new { Success = true, Message = "Role created successfully" });
            }

            return BadRequest(result.Errors);
        }





        // PUT: api/Role/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]

        public async Task<IActionResult> UpdateRole(Guid id, [FromBody] RoleViewModel model)
        {
            if (model == null || string.IsNullOrEmpty(model.Name))
            {
                return BadRequest("Role name cannot be empty");
            }





            var role = await _roleManager.FindByIdAsync(id.ToString());
            if (role == null) return NotFound();

            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdString))
            {
                return Unauthorized("User ID not found in claims.");
            }

            if (!Guid.TryParse(userIdString, out var userId))
            {
                return BadRequest("Invalid user ID format.");
            }


            role.Name = model.Name;
            role.Description = model.Description;
            role.IsActive = model.IsActive;
            role.UpdatedAt = DateTime.Now; // Set UpdatedAt
            role.UpdatedBy = userId; // Set UpdatedBy

            var result = await _roleManager.UpdateAsync(role);

            if (result.Succeeded)
            {
                return Ok(new { Success = true, Message = "Role updated successfully" });
            }

            return BadRequest(result.Errors);
        }





        // DELETE: api/Role/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]

        public async Task<IActionResult> DeleteRole(Guid id)
        {
            var role = await _roleManager.FindByIdAsync(id.ToString());
            if (role == null) return NotFound();

            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdString))
            {
                return Unauthorized("User ID not found in claims.");
            }

            if (!Guid.TryParse(userIdString, out var userId))
            {
                return BadRequest("Invalid user ID format.");
            }


            // Option 1: Soft delete (if supported)
            role.DeletedAt = DateTime.Now;
            role.DeletedBy = userId;
            role.IsDeleted = true; // Assuming your model and logic support soft delete

            var updateResult = await _roleManager.UpdateAsync(role);

            if (updateResult.Succeeded)
            {
                return Ok(new { Success = true, Message = "Role soft deleted successfully" });
            }

            // Option 2: Hard delete
            var result = await _roleManager.DeleteAsync(role);

            if (result.Succeeded)
            {
                return Ok(new { Success = true, Message = "Role deleted successfully" });
            }

            return BadRequest(result.Errors);
        }






    }
}
