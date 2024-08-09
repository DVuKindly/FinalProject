namespace Project.Business.Model
{
    public class UserViewModel
    {
        public Guid Id { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? DisplayName => $"{FirstName} {LastName}";
        public string? Address { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public List<string> UserRoles { get; set; } // Thêm thuộc tính này

        public bool IsActive { get; set; }
    }

}
