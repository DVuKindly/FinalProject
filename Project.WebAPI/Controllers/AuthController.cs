using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Project.Business.Model;
using Project.Business.Service.Auth;

namespace Project.WebAPI.Controllers
{
    [ApiController]
    [Route("auth")]
    [Produces("application/json")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthController"/> class.
        /// </summary>
        /// <param name="authService">The authentication service.</param>
        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// Handles the login request.
        /// </summary>
        /// <param name="loginViewModel">The login view model.</param>
        /// <returns>The login response view model.</returns>
        //Chỉnh sửa thông báo lỗi

        [HttpPost("login")]
        //[ProducesResponseType(typeof(List<LoginResponseViewModel>), StatusCodes.Status200OK)]
        //[ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Login(LoginViewModel loginViewModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var result = await _authService.LoginAsync(loginViewModel);

                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                if (ex.Message.Contains("The user does not exist"))
                {
                    return Unauthorized(new { message = "Invalid username or password" });
                }
                else if (ex.Message.Contains("Email is not confirmed"))
                {
                    return StatusCode(403, new { message = "Email not confirmed. Please verify your email before logging in." });
                }
                else if (ex.Message.Contains("banned"))
                {
                    return StatusCode(403, new { message = "Your account has been banned." });
                }
                else if (ex.Message.Contains("incorrect"))
                {
                    return Unauthorized(new { message = "Invalid username or password" });
                }
                else
                {
                    return StatusCode(500, new { message = "An error occurred. Please try again." });
                }
            }
        }



        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            // Kiểm tra nếu người dùng đã đăng nhập hay chưa
            var user = HttpContext.User;
            if (user == null || !user.Identity.IsAuthenticated)
            {
                return BadRequest("User is not logged in.");
            }

            try
            {
                await _authService.LogoutAsync();
                return Ok("Logout successfully");
            }
            catch (Exception ex)
            {
                // Log lỗi hoặc xử lý tùy theo yêu cầu của bạn
                return StatusCode(500, "An error occurred during logout.");
            }
        }


        /// <summary>
        /// Handles the register request.
        /// </summary>
        /// <param name="registerViewModel">The register view model.</param>
        /// <returns>The login response view model.</returns>
        [HttpPost("register")]
        [ProducesResponseType(typeof(List<LoginResponseViewModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Register(RegisterViewModel registerViewModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _authService.RegisterAsync1(registerViewModel);

            if (result == null)
            {
                return BadRequest();
            }

            return Ok(result);
        }

        [HttpGet("confirmemail")]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(token))
            {
                return BadRequest("User ID and token are required.");
            }

            var result = await _authService.ConfirmEmailAsync(userId, token);
            if (result)
            {
                return Ok("Email confirmed successfully.");
            }

            return BadRequest("Error confirming email.");
        }


        //[HttpGet("confirmemail")]
        //public async Task<IActionResult> ConfirmEmail(string userId, string token)
        //{
        //    if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(token))
        //    {
        //        return BadRequest("User ID and token are required.");
        //    }

        //    var result = await _authService.ConfirmEmailAsync(userId, token);
        //    if (result)
        //    {
        //        return Ok("Email confirmed successfully.");
        //    }

        //    return BadRequest("Error confirming email.");
        //}


        [HttpGet("resetpass")]
        public async Task<IActionResult> ResetPassword(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return BadRequest("Please input your email.");
            }

            await _authService.ForgotPasswordAsync(email);

            return Ok("Check your email.");
        }



        [HttpPost("ResetPasswordAfterForgot")]
        public async Task<IActionResult> ResetPasswordAfterForgot(ResetPasswordViewModel resetPasswordViewModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var result = await _authService.ResetPasswordAsync(resetPasswordViewModel);

            if (result)
            {
                return Ok("ResetPasswordAfterForgot changed successfully.");
            }

            return BadRequest("Error changing ResetPasswordAfterForgot.");

        }



        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordViewModel changePasswordViewModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _authService.ChangePasswordAsync(changePasswordViewModel);

            if (result)
            {
                return Ok("Password changed successfully.");
            }

            return BadRequest("Error changing password.");
        }







    }
}
