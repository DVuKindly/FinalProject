namespace Project.Business.Model
{
    public class AdvancedCandidateReportResult
    {
        public int TotalCandidates { get; set; }
        public int CandidatesWithGPAOverMinGPA { get; set; } = 0;
        public int CandidatesWithSpecificSkills { get; set; } = 0;
        public int CandidatesWithSpecificLanguage { get; set; } = 0;
        public int CandidatesWithSpecificMajor { get; set; } = 0;
        public int CandidatesPass { get; set; } = 0;
        public int CandidatesNotPass { get; set; } = 0;
        public int CandidatesInProgress { get; set; } = 0;
        public int CandidatesTest { get; set; } = 0;
        public int CandidatesRecall { get; set; } = 0;
        public int CandidatesFollowUp { get; set; } = 0;
        public double PassRate { get; set; } = 0.0;
        public double NotPassRate { get; set; } = 0.0;
        public double InProgressRate { get; set; } = 0.0;
        public double TestRate { get; set; } = 0.0;
        public double RecallRate { get; set; } = 0.0;
        public double FollowUpRate { get; set; } = 0.0;
    }
}
