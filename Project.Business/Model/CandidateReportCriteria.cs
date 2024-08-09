namespace Project.Business.Model
{
    public class CandidateReportCriteria
    {
        public double? MinGPA { get; set; }
        public string Skills { get; set; }
        public string Language { get; set; }
        public string Major { get; set; }
        public int? MonthAplicationDate { get; set; } // Tháng
        public int? YearAplicationDate { get; set; }  // Năm
    }
}
