using System.ComponentModel.DataAnnotations;

namespace Project.Business.Model
{
    public class UserCreateViewModel
    {
        [Required(ErrorMessage = "First Name is required")]
        [StringLength(50, ErrorMessage = "First Name must be between 3 and 50 characters", MinimumLength = 3)]
        public  string FirstName { get; set; }

        [Required(ErrorMessage = "Last Name is required")]
        [StringLength(50, ErrorMessage = "Last Name must be between 3 and 50 characters", MinimumLength = 3)]
        public  string LastName { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public  string Email { get; set; }

        [Required(ErrorMessage = "Address is required")]
        [StringLength(50, ErrorMessage = "Address must be between 3 and 50 characters", MinimumLength = 5)]

        public string Address { get; set; }

        [Required(ErrorMessage = "Username is required")]
        [StringLength(50, ErrorMessage = "Username must be between 3 and 50 characters", MinimumLength = 3)]
        public  string UserName { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [StringLength(50, ErrorMessage = "Password must be between 8 and 50 characters", MinimumLength = 8)]
        public  string Password { get; set; }

        [Required(ErrorMessage = "Confirm Password is required")]
        [Compare("Password", ErrorMessage = "Password and Confirm Password must match")]
        public  string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "Date of Birth is required")]
        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }

        [Required(ErrorMessage = "Phone Number is required")]
        [StringLength(50, ErrorMessage = "Phone Number must be between 3 and 50 characters", MinimumLength = 3)]
        public  string PhoneNumber { get; set; }

        public bool IsActive { get; set; }
    }
}
