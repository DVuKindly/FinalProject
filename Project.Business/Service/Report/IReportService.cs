using Project.Business.Model;
using System.Threading.Tasks;

namespace Project.Business.Service.Report
{
    public interface IReportService
    {
        Task<AdvancedCandidateReportResult> GenerateCandidateStatisticsAsync(CandidateReportCriteria criteria);
        Task<AdvancedEventReportResult> GenerateEventStatisticsAsync(EventReportCriteria criteria);
    }
}
