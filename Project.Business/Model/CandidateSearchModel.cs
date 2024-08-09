using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Business.Model
{
    public class CandidateSearchModel
    {
        public string? FullName { get; set; }
        public string? EventName { get; set; }
        public string? Channel { get; set; }
        public string? University { get; set; }
        public int? GraduationYearFrom { get; set; }
        public int? GraduationYearTo { get; set; }
        public double? GPAFrom { get; set; }
        public double? GPATo { get; set; }
        public string? ApplyJob { get; set; }
        public string? Skills { get; set; }
        public string? Languages { get; set; }
        public string? WorkingTime { get; set; }
        public bool? Gender { get; set; }
        public string? Recer { get; set; }
        public string? Status { get; set; }
        public bool? Talent { get; set; }
    }

}
