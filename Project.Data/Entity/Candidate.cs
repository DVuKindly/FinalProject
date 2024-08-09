using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Data.Entity
{
    [Table("Candidate")]
    public class Candidate
    {
        [Key]
        public Guid CandidateId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public DateTime DateOfBirth { get; set; } // New attribute for Date of Birth
        public string PhoneNo { get; set; }
        public string Address { get; set; } // New attribute
        public bool Gender { get; set; } // New attribute
        public string University { get; set; }
        public string Major { get; set; }
        public DateTime GraduationDate { get; set; }
        public double GPA { get; set; }
        public string Skills { get; set; }
        public string Languages { get; set; }
        public string CVLink { get; set; }
        public string ApplyJob { get; set; } // New attribute
        public DateTime ApplicationDate { get; set; } // New attribute
        public string WorkingTime { get; set; } // New attribute
        public bool Talent { get; set; } // New attribute
        public string Channel { get; set; } // New attribute
        public string Recer { get; set; } // New attribute
        public string Note { get; set; } // New attribute
        public string Status { get; set; } // New attribute
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }

        // Foreign key for Event (if applicable)
        public Guid? EventId { get; set; }
        public virtual Event Event { get; set; }

        // Navigation properties for history tables
        public virtual ICollection<SourcingHistory> SourcingHistories { get; set; }
        public virtual ICollection<UpdateHistory> UpdateHistories { get; set; }
        public virtual ICollection<RemarkHistory> RemarkHistories { get; set; }
        public virtual ICollection<AuditTrail> AuditTrails { get; set; }
    }
}
