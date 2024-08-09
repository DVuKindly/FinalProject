using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Project.Business.Model;
using Project.Business.Service;
using Project.Business.Service.Paginiated;
using Project.Data.Entity;
using System.IO;

namespace Project.WebAPI.Controllers
{
    [ApiController]
    [Route("User")]
    [Produces("application/json")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly UserManager<User> _userManager;
        public static int PAGE_SIZE = 2;

        public UsersController(IUserService userService, UserManager<User> userManager)
        {
            _userService = userService;
            _userManager = userManager;
        }

       
        [HttpGet]
     //   [Authorize(Roles = "Admin, HR Leader")]
        [ProducesResponseType(typeof(List<UserViewModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetUsers(string? search = null, bool? isActive = null, int page = 1)
        {
            IEnumerable<User> users;

            if (string.IsNullOrEmpty(search) && !isActive.HasValue)
            {
                // Await the task and retrieve the list of users
                users = await _userService.GetAllAsync();
            }
            else
            {
                // Await the task and retrieve the list of users
                users = await _userService.GetAllAsync();

                // Apply search filter
                if (!string.IsNullOrEmpty(search))
                {
                    search = search.ToLower();
                    users = users.Where(u =>
                        (u.FirstName != null && u.FirstName.ToLower().Contains(search)) ||
                        (u.LastName != null && u.LastName.ToLower().Contains(search)) ||
                        (u.FirstName != null && u.LastName != null &&
                         (u.FirstName.ToLower() + " " + u.LastName.ToLower()).Contains(search))
                    ).ToList();
                }

                // Apply IsActive filter
                if (isActive.HasValue)
                {
                    users = users.Where(u => u.IsActive == isActive).ToList();
                }
            }

            // Apply pagination
            var paginatedUsers = PaginatedList<User>.Create(users.AsQueryable(), page, PAGE_SIZE);

            // Prepare user view models
            var userViewModels = new List<UserViewModel>();
            foreach (var user in paginatedUsers)
            {
                var roles = await _userManager.GetRolesAsync(user);
                var userViewModel = new UserViewModel
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email ?? string.Empty,
                    Address = user.Address,
                    UserName = user.UserName ?? string.Empty,
                    PhoneNumber = user.PhoneNumber ?? string.Empty,
                    IsActive = user.IsActive ?? false,
                    UserRoles = roles.ToList()
                };

                userViewModels.Add(userViewModel);
            }

            return Ok(new
            {
                Users = userViewModels,
                PageIndex = paginatedUsers.PageIndex,
                TotalPages = paginatedUsers.TotalPages
            });
        }







        [HttpPut("UpdateIsActive")]
      //  [Authorize(Roles = "Admin")]

        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateIsActive([FromBody] UpdateIsActiveViewModel model)
        {
            if (model == null || model.UserId == Guid.Empty)
            {
                return BadRequest("Invalid user ID.");
            }

            var result = await _userService.UpdateIsActiveAsync(model.UserId, model.IsActive);
            if (result)
            {
                return Ok(new { Success = true, Message = "IsActive status updated successfully." });
            }

            return BadRequest("Failed to update IsActive status.");
        }



        [HttpGet("{id}")]
      /// / [Authorize(Roles = "Admin, HR Leader")]

        //  [Authorize(Roles = "Admin, Editor")]
        [ProducesResponseType(typeof(UserViewModel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetUser(Guid id)
        {
            var user = await _userService.GetByIdAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            var userViewModel = new UserViewModel
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email ?? string.Empty,
                Address = user.Address,

                UserName = user.UserName ?? string.Empty,
                PhoneNumber = user.PhoneNumber ?? string.Empty,
                IsActive = user.IsActive ?? false
            };
            return Ok(userViewModel);
        }



        [HttpGet("export")]
      //  [Authorize(Roles = "Admin, HR Leader")]

        public async Task<IActionResult> ExportToExcel(string nameFile)
        {
            var stream = await _userService.ExportUsersToExcelAsync();

            if (!string.IsNullOrEmpty(nameFile))
            {

                // Return the Excel file as a download
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", nameFile);
            }
            else
            {
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Users.xlsx");

            }

        }



        [HttpPost("SetRole")]
       /// [Authorize(Roles = "Admin")]

        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SetRole([FromBody] SetRoleViewModel model)
        {
            if (model == null || model.UserId == Guid.Empty || string.IsNullOrEmpty(model.RoleName))
            {
                return BadRequest("Invalid user ID or role name.");
            }

            var result = await _userService.AddUserToRoleAsync(model.UserId, model.RoleName);
            if (result)
            {
                return Ok(new { Success = true, Message = "Role assigned successfully." });
            }

            return BadRequest("Failed to assign role. The role may already be assigned or other errors occurred.");
        }

        [HttpDelete("{userId}/roles/{roleName}")]
       /// [Authorize(Roles = "Admin")]

        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteUserRole(Guid userId, string roleName)
        {
            if (userId == Guid.Empty || string.IsNullOrEmpty(roleName))
            {
                return BadRequest("Invalid user ID or role name.");
            }

            var result = await _userService.DeleteUserRoleAsync(userId, roleName);

            if (!result)
            {
                return BadRequest("Failed to remove role from user.");
            }

            return Ok(new { Success = true, Message = "Role removed successfully." });
        }



        [AllowAnonymous]
        [HttpPost("/changePassword")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel changePasswordViewModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            bool result = await _userService.ChangePasswordAsync(changePasswordViewModel);

            if (!result)
            {
                return BadRequest();
            }

            return Ok(result);
        }

        [HttpPost]
     // [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Consumes("application/json")]
        public async Task<IActionResult> CreateUser(UserCreateViewModel userCreateViewModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            bool result = await _userService.CreateUserAsync(userCreateViewModel);

            if (!result)
            {
                return BadRequest();
            }

            return Ok(result);
        }


        [HttpPut("{id}")]
     /// [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Consumes("application/json")]
        public async Task<IActionResult> UpdateUser(Guid id, UserEditViewModel userEditViewModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            bool result = await _userService.UpdateUserAsync(id, userEditViewModel);

            if (!result)
            {
                return BadRequest();
            }

            return Ok(result);
        }


        [HttpDelete("{id}")]
      /// [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            bool result = await _userService.DeleteAsync(id);

            if (!result)
            {
                return BadRequest();
            }

            return Ok(result);
        }
    }
}
