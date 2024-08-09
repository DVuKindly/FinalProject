
using ASP_PROJECT_OJT.Business.Service;
using Microsoft.AspNetCore.Identity;
using Project.Business.Model;
using Project.Data.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Business.Service
{
    public interface IUserService : IBaseService<User>
    {
        Task<bool> ChangePasswordAsync(ChangePasswordViewModel changePasswordViewModel);

        Task<bool> CreateUserAsync(UserCreateViewModel userCreateViewModel);

        Task<bool> UpdateUserAsync(Guid id, UserEditViewModel userEditViewModel);
        Task<bool> AddUserToRoleAsync(Guid userId, string roleName);
        Task<bool> UpdateIsActiveAsync(Guid userId, bool isActive); // Bổ sung phương thức này
        Task<bool> DeleteUserRoleAsync(Guid userId, string roleName);
        Task<MemoryStream> ExportUsersToExcelAsync();

        Task<bool> DeleteUserAsync(Guid userId);
        //Task<bool> DeleteUserAsync(Guid id);

        //Task<IEnumerable<UserViewModel>> GetAllUsersAsync();


        //Task<UserViewModel?> GetUserByIdAsync(Guid id);


    }
}
