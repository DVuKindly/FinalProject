using Project.Data.Entity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Business.Model
{
    public class CandidateCreateViewModel
    {
        public Guid CandidateId { get; set; }
        public string FullName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }

        [Required]
        public string PhoneNo { get; set; }

        [Required]
        public string Address { get; set; }

        [Required]
        public bool Gender { get; set; }

        [Required]
        public string University { get; set; }

        [Required]
        public string Major { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime GraduationDate { get; set; }

        [Required]
        [Range(0, 4.0)]
        public double GPA { get; set; }

        public string Skills { get; set; }
        public string Languages { get; set; }
        public string CVLink { get; set; }

        [Required]
        public string ApplyJob { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime ApplicationDate { get; set; }

        [Required]
        public string WorkingTime { get; set; }

        public bool Talent { get; set; }
        public string Channel { get; set; }
        public string Recer { get; set; }
        public string Note { get; set; }
        public string Status { get; set; }

        public Guid? EventId { get; set; }
    }
}