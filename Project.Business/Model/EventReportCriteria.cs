using System;

namespace Project.Business.Model
{
    public class EventReportCriteria
    {
        public string EventName { get; set; }
        public int? MinTarget { get; set; }
        public int? MaxTotalParticipants { get; set; }
    }
}
