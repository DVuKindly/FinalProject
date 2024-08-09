using Microsoft.EntityFrameworkCore;
using Project.Business.Infrastructure;
using Project.Business.Model;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Project.Data.Entity;

namespace Project.Business.Service.Report
{
    public class ReportService : IReportService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ReportService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // Phương pháp giả lập tính toán ngày tháng cho ứng viên
        private DateTime SomeDateCalculationMethod(Candidate candidate)
        {
            // Giả lập tính toán ngày tháng dựa trên dữ liệu hiện có
            // Ví dụ: sử dụng ngày ứng tuyển, hoặc một ngày khác từ dữ liệu của ứng viên
            return candidate.ApplicationDate; // Hoặc một logic khác phù hợp
        }

        public async Task<AdvancedCandidateReportResult> GenerateCandidateStatisticsAsync(CandidateReportCriteria criteria)
        {
            // Lấy tất cả các dữ liệu từ repository
            var allCandidates = await _unitOfWork.CandidateRepository.GetQuery().ToListAsync();

            // Lọc sau khi lấy dữ liệu
            if (criteria.YearAplicationDate.HasValue)
            {
                allCandidates = allCandidates
                    .Where(c => SomeDateCalculationMethod(c).Year == criteria.YearAplicationDate.Value)
                    .ToList();
            }

            if (criteria.MonthAplicationDate.HasValue)
            {
                allCandidates = allCandidates
                    .Where(c => SomeDateCalculationMethod(c).Month == criteria.MonthAplicationDate.Value)
                    .ToList();
            }

            // Tạo đối tượng kết quả
            var result = new AdvancedCandidateReportResult
            {
                TotalCandidates = allCandidates.Count
            };

            // Áp dụng các tiêu chí lọc và cập nhật kết quả
            if (criteria.MinGPA.HasValue)
            {
                result.CandidatesWithGPAOverMinGPA = allCandidates.Count(c => c.GPA >= criteria.MinGPA.Value);
            }

            if (!string.IsNullOrEmpty(criteria.Skills))
            {
                result.CandidatesWithSpecificSkills = allCandidates.Count(c => c.Skills.Contains(criteria.Skills));
            }

            if (!string.IsNullOrEmpty(criteria.Language))
            {
                result.CandidatesWithSpecificLanguage = allCandidates.Count(c => c.Languages.Contains(criteria.Language));
            }

            if (!string.IsNullOrEmpty(criteria.Major))
            {
                result.CandidatesWithSpecificMajor = allCandidates.Count(c => c.Major.Contains(criteria.Major));
            }

            // Thêm thống kê số người theo trạng thái khác nhau và tính tỉ lệ
            result.CandidatesPass = allCandidates.Count(c => c.Status == "Pass");
            result.CandidatesNotPass = allCandidates.Count(c => c.Status == "Not qualify");
            result.CandidatesInProgress = allCandidates.Count(c => c.Status == "In-Progress");
            result.CandidatesTest = allCandidates.Count(c => c.Status == "Test");
            result.CandidatesRecall = allCandidates.Count(c => c.Status == "Recall");
            result.CandidatesFollowUp = allCandidates.Count(c => c.Status == "Follow-up");

            if (result.TotalCandidates > 0)
            {
                result.PassRate = (double)result.CandidatesPass / result.TotalCandidates * 100;
                result.NotPassRate = (double)result.CandidatesNotPass / result.TotalCandidates * 100;
                result.InProgressRate = (double)result.CandidatesInProgress / result.TotalCandidates * 100;
                result.TestRate = (double)result.CandidatesTest / result.TotalCandidates * 100;
                result.RecallRate = (double)result.CandidatesRecall / result.TotalCandidates * 100;
                result.FollowUpRate = (double)result.CandidatesFollowUp / result.TotalCandidates * 100;
            }

            return result;
        }

        public async Task<AdvancedEventReportResult> GenerateEventStatisticsAsync(EventReportCriteria criteria)
        {
            var eventsQuery = _unitOfWork.EventRepository.GetQuery();

            if (!string.IsNullOrEmpty(criteria.EventName))
            {
                eventsQuery = eventsQuery.Where(e => e.EventName.Contains(criteria.EventName));
            }

            var events = await eventsQuery.ToListAsync();

            // Convert and filter events based on MinTarget and MaxTotalParticipants
            int minTarget = criteria.MinTarget.HasValue ? criteria.MinTarget.Value : 0;
            int maxTotalParticipants = criteria.MaxTotalParticipants.HasValue ? criteria.MaxTotalParticipants.Value : int.MaxValue;

            var filteredEvents = events.Where(e =>
            {
                bool isTargetValid = int.TryParse(e.Target, out int target);
                return isTargetValid && target >= minTarget && e.TotalParticipants <= maxTotalParticipants;
            }).ToList();

            var result = new AdvancedEventReportResult
            {
                TotalEvents = events.Count,
                EventsWithSpecificTarget = filteredEvents.Count(e =>
                {
                    bool isTargetValid = int.TryParse(e.Target, out int target);
                    return isTargetValid && target >= minTarget;
                }),
                EventsWithSpecificTotalParticipants = filteredEvents.Count(e => e.TotalParticipants <= maxTotalParticipants)
            };

            return result;
        }
    }
}
