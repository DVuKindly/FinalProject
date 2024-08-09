namespace Project.Business.Model
{
    public class ChangePasswordViewModel
    {
        public Guid Id { get; set; }

        public  string? UserName { get; set; }

        public  string? CurrentPassword { get; set; }

        public  string? NewPassword { get; set; }

        public  string? ConfirmPassword { get; set; }

    }
}
