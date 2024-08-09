using Project.Data.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Business.Model
{
    public class EventCreationViewModel
    {
        public Guid EventId { get; set; }
        public string EventName { get; set; }
        public string Channel { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Partner { get; set; }
        public string Target { get; set; }
        public int TotalParticipants { get; set; }
        public string Notes { get; set; }
        public string Status { get; set; }
        
    }
}
