using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Business.Model
{
    public class CandidateModelsReviewImport
    {
     
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
        public Guid EventId { get; set; } 

        public string? ReviewImport{ get; set; } // New attribute

    }
}
