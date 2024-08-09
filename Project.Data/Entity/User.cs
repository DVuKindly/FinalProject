using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Data.Entity
{
    [Table("User")]
    public class User : IdentityUser<Guid>
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? DisplayName { get; set; }

        public DateTime DateOfBirth { get; set; }
        public string Address { get; set; }
        public string? Avatar { get; set; }
        public bool? IsActive { get; set; } = true;

        // New fields for audit information
        public DateTime? CreatedAt { get; set; }
        public Guid? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public Guid? UpdatedBy { get; set; }
        public DateTime? DeletedAt { get; set; }
        public Guid? DeletedBy { get; set; }

        public ICollection<UserRole> UserRoles { get; set; }
        public virtual ICollection<Event> Events { get; set; } = new List<Event>();




    }
}
