using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Project.Business.Model;
using Project.Business.Service.Email;
using Project.Data.Entity;
using QuizApp.Business.Helper;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace Project.Business.Service.Auth
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly IEmailSender _emailSender;
        private readonly ILogger<AuthService> _logger; // Thêm logger

        public AuthService(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            IConfiguration configuration,
            IEmailSender emailSender,
            ILogger<AuthService> logger) // Nhận logger qua constructor
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _emailSender = emailSender;
            _logger = logger; // Khởi tạo logger
        }

        //Chỉnh sửa thông báo lỗi login và hiển thị tên người dùng
        public async Task<LoginResponseViewModel> LoginAsync(LoginViewModel loginViewModel)
        {
            var existingUser = await _userManager.FindByNameAsync(loginViewModel.UserName);

            if (existingUser == null)
            {
                throw new ArgumentException("The user does not exist.");
            }

            if (!existingUser.EmailConfirmed)
            {
                throw new ArgumentException("Email is not confirmed. Please confirm your email before logging in.");
            }

            if (existingUser.IsActive == false)
            {
                throw new ArgumentException("Your account has been banned.");
            }

            var result = await _signInManager.CheckPasswordSignInAsync(existingUser, loginViewModel.Password, false);

            if (!result.Succeeded)
            {
                throw new ArgumentException("The password is incorrect.");
            }

            var userViewModel = new UserViewModel
            {
                Id = existingUser.Id,
                FirstName = existingUser.FirstName ?? string.Empty,
                LastName = existingUser.LastName ?? string.Empty,
                Email = existingUser.Email ?? string.Empty,
                Address = existingUser.Address,
                UserName = existingUser.UserName ?? string.Empty,
                PhoneNumber = existingUser.PhoneNumber ?? string.Empty,
                IsActive = existingUser.IsActive ?? false
            };

            var userJson = GetSerializeObject(userViewModel);

            var roles = await _userManager.GetRolesAsync(existingUser);

            var claims = new List<Claim>
    {
        new(ClaimTypes.NameIdentifier, existingUser.Id.ToString()),
        new(ClaimTypes.Name, existingUser.UserName ?? string.Empty),
        new(ClaimTypes.Email, existingUser.Email ?? string.Empty),
        new(ClaimTypes.GivenName, existingUser.FirstName ?? string.Empty),
        new(ClaimTypes.Surname, existingUser.LastName ?? string.Empty),
        new(ClaimTypes.Role, string.Join(",", roles))
    };

            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Secret"] ?? "congdinh2021@gmail.com"));

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:ValidIssuer"],
                audience: _configuration["Jwt:ValidAudience"],
                claims: claims,
                expires: _configuration["Jwt.ExpirationInMinutes"] == null ? DateTime.Now.AddDays(1) : DateTime.Now.AddMinutes(Convert.ToDouble(_configuration["Jwt.ExpirationInMinutes"])),
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
            );

            return new LoginResponseViewModel
            {
                UserInformation = userJson,
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                Expires = token.ValidTo,
                UserName = existingUser.UserName
            };
        }
        public async Task<LoginResponseViewModel> RegisterAsync(RegisterViewModel registerViewModel)
        {
            var existingUser = await _userManager.FindByNameAsync(registerViewModel.UserName);
            if (existingUser != null)
            {
                throw new ArgumentException("The user already exists.");
            }

            var user = new User
            {
                FirstName = registerViewModel.FirstName,
                LastName = registerViewModel.LastName,
                Email = registerViewModel.Email,
                UserName = registerViewModel.UserName,
                Address = registerViewModel.Address,
                PhoneNumber = registerViewModel.PhoneNumber,
                DateOfBirth = registerViewModel.DateOfBirth,
                IsActive = registerViewModel.IsActive,
                CreatedAt = DateTime.Now,

            };

            var result = await _userManager.CreateAsync(user, registerViewModel.Password);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(error => error.Description);
                throw new ArgumentException($"The user could not be created. Errors: {string.Join(", ", errors)}");
            }

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var userId = user.Id.ToString();

            //        var emailMessage = $@"
            //<p>Thank you for registering. Please use the following information to confirm your account:</p>
            //<p>User ID: {userId}</p>
            //<p>Token: {token}</p>
            //<p>You can confirm your account by visiting the following link and entering these details:</p>
            //<p><a href='{_configuration["AppUrl"]}/confirm-email'>Confirm Your Account</a></p>";

            await _emailSender.SendEmailAsync(user.Email, "[Notification] Confirm Your Account", EmailTemplate.GenerateConfirmationEmail(userId, token, _configuration["AppUrl"]));

            return new LoginResponseViewModel
            {
                UserInformation = "Please confirm your email",
                Token = string.Empty,
                Expires = DateTime.MinValue
            };
        }



        public async Task<LoginResponseViewModel> RegisterAsync1(RegisterViewModel registerViewModel)
        {
            var existingUser = await _userManager.FindByNameAsync(registerViewModel.UserName);
            if (existingUser != null)
            {
                throw new ArgumentException("The user already exists.");
            }

            var user = new User
            {
                FirstName = registerViewModel.FirstName,
                LastName = registerViewModel.LastName,
                Email = registerViewModel.Email,
                UserName = registerViewModel.UserName,
                Address = registerViewModel.Address,
                PhoneNumber = registerViewModel.PhoneNumber,
                DateOfBirth = registerViewModel.DateOfBirth,
                IsActive = registerViewModel.IsActive,
                CreatedAt = DateTime.Now,
            };

            var result = await _userManager.CreateAsync(user, registerViewModel.Password);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(error => error.Description);
                throw new ArgumentException($"The user could not be created. Errors: {string.Join(", ", errors)}");
            }

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var confirmationLink = $"{_configuration["AppUrl"]}/confirmemail?userId={user.Id}&token={token}";

            await _emailSender.SendEmailAsync(user.Email, "[Notification] Confirm Your Account", EmailTemplate.GenerateConfirmationEmail(user.UserName, confirmationLink));

            return new LoginResponseViewModel
            {
                UserInformation = "Please confirm your email",
                Token = string.Empty,
                Expires = DateTime.MinValue
            };
        }


        public async Task<bool> ConfirmEmailAsync(string userId, string token)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                throw new ArgumentException("Invalid user ID.");
            }
            user.UpdatedAt = DateTime.Now;
            await _userManager.UpdateAsync(user);
            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (result.Succeeded)
            {
                user.EmailConfirmed = true;
                await _userManager.UpdateAsync(user);
                return true;
            }

            return false;
        }






        public async Task ForgotPasswordAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                throw new ArgumentException("Account not exist.");
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var callbackUrl = $"{_configuration["AppUrl"]}/reset-password?token={WebUtility.UrlEncode(token)}&email={email}";

            //        var emailMessage = $@"
            //<p>You have requested to reset your password. Please click the link below to reset your password:</p>
            //<p>Token: {token}</p>

            //<p><a href='{callbackUrl}'>Reset Password</a></p>
            //<p>If you did not request this, please ignore this email.</p>";

            await _emailSender.SendEmailAsync(email, "[Notification] Reset Your Password", EmailTemplate.sentMail(callbackUrl, token));
        }

        public async Task LogoutAsync()
        {
            // Sign out the user by calling SignInManager
            await _signInManager.SignOutAsync();
            // Additional logic if needed, such as logging the event
        }





        public async Task<bool> ChangePasswordAsync(ChangePasswordViewModel changePasswordViewModel)
        {
            var user = await _userManager.FindByIdAsync(changePasswordViewModel.Id.ToString());
            if (user == null)
            {
                throw new ArgumentException("Invalid user ID.");
            }

            if (changePasswordViewModel.NewPassword != changePasswordViewModel.ConfirmPassword)
            {
                throw new ArgumentException("New password and confirm password do not match.");
            }

            var result = await _userManager.ChangePasswordAsync(user, changePasswordViewModel.CurrentPassword, changePasswordViewModel.NewPassword);
            return result.Succeeded;
        }





        public async Task<bool> ResetPasswordAsync(ResetPasswordViewModel model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                throw new ArgumentException("Account (email) not exist.");
            }

            var result = await _userManager.ResetPasswordAsync(user, model.Token, model.NewPassword);
            if (result.Succeeded)
            {
                return true; // Thành công
            }

            throw new ArgumentException("Error resetting password, maybe token or new password is invalid.");
        }




        private string GetSerializeObject(object value)
        {
            var serializeOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };
            return value != null ? JsonSerializer.Serialize(value, serializeOptions) : string.Empty;
        }
    }

}
