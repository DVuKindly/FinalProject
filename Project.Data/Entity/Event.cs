using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Data.Entity
{
    [Table("Event")]
    public class Event
    {
        [Key]
        public Guid EventId { get; set; }
        public string EventName { get; set; }
        public string Channel { get; set; } // Kênh tổ chức (Facebook, Offline event, etc.)
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Partner { get; set; } // Đối tác tham gia
        public string Target { get; set; } // Mục tiêu (ví dụ: 100 CVs, 20 tuyển dụng)
        public int TotalParticipants { get; set; } // Tổng số người tham gia
        public string Notes { get; set; } // Ghi chú
        public string Status { get; set; } // Trạng thái sự kiện (New, In-progress, Ended, Cancelled)

        // Navigation property cho các CandidateRecords liên quan
        public virtual ICollection<Candidate> candidate { get; set; } = new List<Candidate>();
        public DateTime? CreatedAt { get; set; }

        public Guid? CreatedBy { get; set; }
        public virtual User Creator { get; set; }

        public DateTime? UpdatedAt { get; set; }
        public Guid? UpdatedBy { get; set; }
        public DateTime? DeletedAt { get; set; }
        public Guid? DeletedBy { get; set; }
        public bool IsDeleted { get; set; } = false;

        // Navigation property cho các CandidateRecords liên quan


    }
}
