using ASP_PROJECT_OJT.Business.Service;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using Project.Business.Infrastructure;
using Project.Business.Model;
using Project.Data.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Project.Business.Service
{
    public class UserService : BaseService<User>, IUserService
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Role> _roleManager;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserService(IUnitOfWork unitOfWork, ILogger<UserService> logger, UserManager<User> userManager, RoleManager<Role> roleManager, IHttpContextAccessor httpContextAccessor)
            : base(unitOfWork, logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _httpContextAccessor = httpContextAccessor; // Khởi tạo HttpContextAccessor
        }

        public async Task<bool> ChangePasswordAsync(ChangePasswordViewModel changePasswordViewModel)
        {
            if (changePasswordViewModel == null)
            {
                _logger.LogError("Change password view model is null.");
                return false;
            }

            var user = await _userManager.FindByNameAsync(changePasswordViewModel.UserName);

            if (user == null)
            {
                _logger.LogWarning($"User not found: {changePasswordViewModel.UserName}");
                return false;
            }

            if (changePasswordViewModel.NewPassword != changePasswordViewModel.ConfirmPassword)
            {
                _logger.LogWarning("New password and confirm password do not match.");
                return false;
            }

            var result = await _userManager.ChangePasswordAsync(user, changePasswordViewModel.CurrentPassword, changePasswordViewModel.NewPassword);

            if (result.Succeeded)
            {
                var UserString = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!Guid.TryParse(UserString, out var UserId))
                {
                    _logger.LogError("Current user ID is not a valid GUID.");
                    throw new FormatException("Current user ID is not a valid GUID.");
                }

                user.UpdatedAt = DateTime.Now;
                user.UpdatedBy = UserId;
                await _userManager.UpdateAsync(user);

                _logger.LogInformation($"Password changed for user: {user.UserName}");
                return true;
            }

            foreach (var error in result.Errors)
            {
                _logger.LogError(error.Description);
            }

            return false;
        }

        public async Task<bool> CreateUserAsync(UserCreateViewModel userCreateViewModel)
        {
            var UserString = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(UserString))
            {
                _logger.LogError("Current user ID is not available.");
                throw new KeyNotFoundException("Current user ID is not available.");
            }

            if (!Guid.TryParse(UserString, out var UserId))
            {
                _logger.LogError("Current user ID is not a valid GUID.");
                throw new FormatException("Current user ID is not a valid GUID.");
            }

            var user = new User
            {
                FirstName = userCreateViewModel.FirstName,
                LastName = userCreateViewModel.LastName,
                Email = userCreateViewModel.Email,
                Address = userCreateViewModel.Address,
                UserName = userCreateViewModel.UserName,
                PhoneNumber = userCreateViewModel.PhoneNumber,
                DateOfBirth = userCreateViewModel.DateOfBirth,
                IsActive = userCreateViewModel.IsActive,
                CreatedAt = DateTime.Now,
                CreatedBy = UserId,
                UpdatedAt = DateTime.Now,
                UpdatedBy = UserId
            };

            var result = await _userManager.CreateAsync(user, userCreateViewModel.Password);

            if (result.Succeeded)
            {
                _logger.LogInformation($"User created: {user.UserName}");
                await AddUserToRoleAsync(user.Id, "Recer");
                return true;
            }

            foreach (var error in result.Errors)
            {
                _logger.LogError(error.Description);
            }

            return false;
        }

        public async Task<bool> UpdateIsActiveAsync(Guid userId, bool isActive)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                _logger.LogWarning($"User not found: {userId}");
                return false;
            }

            var UserString = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(UserString, out var UserId))
            {
                _logger.LogError("Current user ID is not a valid GUID.");
                throw new FormatException("Current user ID is not a valid GUID.");
            }

            user.IsActive = isActive;
            user.UpdatedAt = DateTime.Now;
            user.UpdatedBy = UserId;

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                _logger.LogInformation($"User {user.UserName} IsActive status updated to {isActive}");
                return true;
            }

            foreach (var error in result.Errors)
            {
                _logger.LogError(error.Description);
            }

            return false;
        }

        public async Task<bool> UpdateUserAsync(Guid id, UserEditViewModel userEditViewModel)
        {
            var user = await _unitOfWork.UserRepository.GetByIdAsync(id);

            if (user == null)
            {
                _logger.LogWarning($"User not found: {id}");
                return false;
            }

            var UserString = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(UserString))
            {
                _logger.LogError("Current user ID is not available.");
                throw new KeyNotFoundException("Current user ID is not available.");
            }

            if (!Guid.TryParse(UserString, out var UserId))
            {
                _logger.LogError("Current user ID is not a valid GUID.");
                throw new FormatException("Current user ID is not a valid GUID.");
            }

            user.FirstName = userEditViewModel.FirstName;
            user.LastName = userEditViewModel.LastName;
            user.Address = userEditViewModel.Address;
            user.PhoneNumber = userEditViewModel.PhoneNumber;
            user.DateOfBirth = userEditViewModel.DateOfBirth;
            user.IsActive = userEditViewModel.IsActive;
            user.UpdatedAt = DateTime.Now;
            user.UpdatedBy = UserId;

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                _logger.LogInformation($"User updated: {user.UserName}");
                return true;
            }

            foreach (var error in result.Errors)
            {
                _logger.LogError(error.Description);
            }

            return false;
        }

        public async Task<bool> DeleteUserAsync(Guid userId)
        {
            var user = await _unitOfWork.UserRepository.GetByIdAsync(userId);

            if (user == null)
            {
                _logger.LogWarning($"User not found: {userId}");
                return false;
            }

            var UserString = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(UserString))
            {
                _logger.LogError("Current user ID is not available.");
                throw new KeyNotFoundException("Current user ID is not available.");
            }

            if (!Guid.TryParse(UserString, out var UserId))
            {
                _logger.LogError("Current user ID is not a valid GUID.");
                throw new FormatException("Current user ID is not a valid GUID.");
            }

            user.DeletedAt = DateTime.Now;
            user.DeletedBy = UserId;
            user.IsActive = false;

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                _logger.LogInformation($"User {user.UserName} marked as deleted by {UserString}.");
                return true;
            }

            foreach (var error in result.Errors)
            {
                _logger.LogError(error.Description);
            }

            return false;
        }

        public async Task<bool> AddUserToRoleAsync(Guid userId, string roleName)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                _logger.LogWarning($"User not found: {userId}");
                return false;
            }

            var role = await _roleManager.FindByNameAsync(roleName);
            if (role == null)
            {
                _logger.LogWarning($"Role not found: {roleName}");
                return false;
            }

            if (await _userManager.IsInRoleAsync(user, role.Name))
            {
                _logger.LogWarning($"User {user.UserName} already has role {role.Name}");
                return false;
            }

            var UserString = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(UserString))
            {
                _logger.LogError("Current user ID is not available.");
                throw new KeyNotFoundException("Current user ID is not available.");
            }

            if (!Guid.TryParse(UserString, out var UserId))
            {
                _logger.LogError("Current user ID is not a valid GUID.");
                throw new FormatException("Current user ID is not a valid GUID.");
            }

            var result = await _userManager.AddToRoleAsync(user, role.Name);
            if (result.Succeeded)
            {
                var userRole = new UserRole
                {
                    UserId = user.Id,
                    RoleId = role.Id,
                    CreatedAt = DateTime.Now,
                    CreatedBy = UserId,
                    IsDeleted = false
                };

                _unitOfWork.UserRoleRepository.Add(userRole);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation($"User {user.UserName} added to role {role.Name} by {UserString}.");
                return true;
            }

            foreach (var error in result.Errors)
            {
                _logger.LogError(error.Description);
            }

            return false;
        }

        public async Task<bool> DeleteUserRoleAsync(Guid userId, string roleName)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                throw new ArgumentException("User not found.");
            }

            if (!await _userManager.IsInRoleAsync(user, roleName))
            {
                throw new ArgumentException($"User is not in role {roleName}.");
            }

            var result = await _userManager.RemoveFromRoleAsync(user, roleName);

            if (result.Succeeded)
            {
                _logger.LogInformation($"User {user.UserName} removed from role {roleName}.");
                return true;
            }

            foreach (var error in result.Errors)
            {
                _logger.LogError($"Error removing role {roleName} from user {user.UserName}: {error.Description}");
            }

            return false;
        }

        public async Task<MemoryStream> ExportUsersToExcelAsync()
        {
            var users = _unitOfWork.UserRepository.GetQuery();
            var userViewModels = new List<UserViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);

                var userViewModel = new UserViewModel
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Address = user.Address,
                    DateOfBirth = user.DateOfBirth,
                    UserName = user.UserName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    IsActive = user.IsActive ?? false,
                    UserRoles = roles.ToList()
                };

                userViewModels.Add(userViewModel);
            }

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Users");

                worksheet.Cells[1, 1].Value = "UserName";
                worksheet.Cells[1, 2].Value = "FirstName";
                worksheet.Cells[1, 3].Value = "LastName";
                worksheet.Cells[1, 4].Value = "Email";
                worksheet.Cells[1, 5].Value = "Address";
                worksheet.Cells[1, 6].Value = "PhoneNumber";
                worksheet.Cells[1, 7].Value = "DateOfBirth";
                worksheet.Cells[1, 8].Value = "IsActive";
                worksheet.Cells[1, 9].Value = "Roles";

                for (int i = 0; i < userViewModels.Count; i++)
                {
                    var user = userViewModels[i];
                    worksheet.Cells[i + 2, 1].Value = user.UserName;
                    worksheet.Cells[i + 2, 2].Value = user.FirstName;
                    worksheet.Cells[i + 2, 3].Value = user.LastName;
                    worksheet.Cells[i + 2, 4].Value = user.Email;
                    worksheet.Cells[i + 2, 5].Value = user.Address;
                    worksheet.Cells[i + 2, 6].Value = user.PhoneNumber;
                    worksheet.Cells[i + 2, 7].Value = user.DateOfBirth.ToString("yyyy-MM-dd");
                    worksheet.Cells[i + 2, 8].Value = user.IsActive ? "Activated" : "Deactivated";
                    worksheet.Cells[i + 2, 9].Value = string.Join(", ", user.UserRoles);
                }

                worksheet.Cells.AutoFitColumns();

                var stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;
                return stream;
            }
        }
    }
}
