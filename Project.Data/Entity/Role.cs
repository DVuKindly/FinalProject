using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Data.Entity
{
    [Table("Role")]
    public class Role : IdentityRole<Guid>
    {
        public string? Description { get; set; }
        public bool? IsActive { get; set; } = true;
        public DateTime? CreatedAt { get; set; }
        public Guid? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public Guid? UpdatedBy { get; set; }
        public DateTime? DeletedAt { get; set; }
        public Guid? DeletedBy { get; set; }
        public bool IsDeleted { get; set; } = false;
        public ICollection<UserRole> UserRoles { get; set; }
    }
}
