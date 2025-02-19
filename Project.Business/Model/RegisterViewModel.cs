﻿namespace Project.Business.Model
{
    public class RegisterViewModel
    {
        public  string FirstName { get; set; }

        public  string LastName { get; set; }

        public  string Email { get; set; }

        public string Address { get; set; }



        public string UserName { get; set; }

        public  string Password { get; set; }

        public  string ConfirmPassword { get; set; }

        public DateTime DateOfBirth { get; set; }

        public  string PhoneNumber { get; set; }

        public bool IsActive { get; set; }
    }
}
