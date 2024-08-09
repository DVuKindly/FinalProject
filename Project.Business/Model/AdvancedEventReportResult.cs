using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Business.Model
{
    public class AdvancedEventReportResult
    {
        public int TotalEvents { get; set; }
        public int? EventsWithSpecificTarget { get; set; }
        public int? EventsWithSpecificTotalParticipants { get; set; }
    }
}
