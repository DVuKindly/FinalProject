using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Data.Entity
{
    [Table("UpdateHistory")]
    public class UpdateHistory
    {
        [Key]
        public int HistoryID { get; set; }
        public Guid CandidateID { get; set; }
        public string UpdateDetails { get; set; }
        public string ChangedBy { get; set; }
        public DateTime ChangeTimestamp { get; set; }

        public virtual Candidate candidate { get; set; }
    }
}
