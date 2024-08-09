using Microsoft.AspNetCore.Identity;
using Project.Business.Model;

namespace Project.Business.Service.Auth
{
    public interface IAuthService
    {
        Task<LoginResponseViewModel> LoginAsync(LoginViewModel loginViewModel);
        Task<LoginResponseViewModel> RegisterAsync(RegisterViewModel registerViewModel);
        Task<bool> ConfirmEmailAsync(string userId, string token);
        Task ForgotPasswordAsync(string email);
        Task LogoutAsync();
        Task<bool> ChangePasswordAsync(ChangePasswordViewModel changePasswordViewModel);
        Task<bool> ResetPasswordAsync(ResetPasswordViewModel model);
        Task<LoginResponseViewModel> RegisterAsync1(RegisterViewModel registerViewModel);
    }

}
