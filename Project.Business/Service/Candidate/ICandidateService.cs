using Microsoft.AspNetCore.Http;
using Project.Business.Model;
using Project.Data.Entity;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Project.Business.Service
{
    public interface ICandidateService
    {
        Task<(string previewId, List<CandidateModelsReviewImport> previewResult)> VerifyAndPreviewImportAsync(IFormFile file, Guid eventId, string channel, string cvLink);

        Task ConfirmImportAsync(string previewId);
        void CancelImport(string previewId);
        Task<byte[]> ExportCandidatesToExcelAsync();
        Task<IEnumerable<Candidate>> GetAllCandidates();
        Task<IEnumerable<Candidate>> GetCandidatesByEventId(Guid eventId);
        Task<IEnumerable<Candidate>> AdvancedSearchCandidates(CandidateSearchModel searchModel);
        Task CreateCandidateAsync(CandidateCreateViewModel model);
        Task UpdateCandidateStatusAsync(Guid id, string status);
        Task<Candidate> GetCandidateByIdAsync(Guid id);
        Task DeleteCandidateAsync(Guid id);
        Task UpdateCandidateAsync(CandidateCreateViewModel model);
        Task<IEnumerable<Candidate>> GetAllCandidatesAsync(string searchTerm = null);
    }
}
