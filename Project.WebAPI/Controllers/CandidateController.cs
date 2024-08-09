using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Project.Business.Model;
using Project.Business.Service;
using Project.Business.Service.Paginiated;
using Project.Data.Entity;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Project.WebAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CandidateController : Controller
    {
        private readonly ICandidateService _candidateServices;
        private readonly ILogger<CandidateController> _logger;
        public static int PAGE_SIZE = 2;

        public CandidateController(ICandidateService candidateServices, ILogger<CandidateController> logger)
        {
            _candidateServices = candidateServices;
            _logger = logger;
        }
        [HttpPost("importExcelCandidatePreview")]
        // [Authorize(Roles = "Admin, HR Leader")]
        public async Task<IActionResult> ImportCandidatesPreview(IFormFile file, Guid eventId, string channel)
        {
            if (file == null || file.Length == 0)
                return BadRequest("File is empty");

            if (string.IsNullOrWhiteSpace(channel))
                return BadRequest("Channel must be provided");

            try
            {
                var cvLink = Path.GetFileNameWithoutExtension(file.FileName);
                var (previewId, previewResult) = await _candidateServices.VerifyAndPreviewImportAsync(file, eventId, channel, cvLink);
                return Ok(new { previewId, previewResult });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("confirmImport")]
        public async Task<IActionResult> ConfirmImport([FromBody] string previewId)
        {
            try
            {
                _logger.LogInformation($"Confirming import with preview ID: {previewId}");
                await _candidateServices.ConfirmImportAsync(previewId);
                return Ok("Import confirmed and data saved.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error confirming import with preview ID: {previewId}, Error: {ex.Message}");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpPost("cancelImport")]
        // [Authorize(Roles = "Admin, HR Leader")]
        public IActionResult CancelImport([FromBody] string previewId)
        {
            try
            {
                _logger.LogInformation($"Cancelling import with preview ID: {previewId}");
                _candidateServices.CancelImport(previewId);
                return Ok("Import canceled and data discarded.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error cancelling import with preview ID: {previewId}, Error: {ex.Message}");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("export")]
        // [Authorize(Roles = "Admin, HR Leader")]
        public async Task<IActionResult> ExportToExcel(string? nameFile)
        {
            try
            {
                var stream = await _candidateServices.ExportCandidatesToExcelAsync();

                if (!string.IsNullOrEmpty(nameFile))
                {
                    return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", nameFile);
                }
                else
                {
                    return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Candidates.xlsx");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("advancedSearch")]
        // [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdvancedSearchCandidates([FromBody] CandidateSearchModel searchModel, int page = 1)
        {
            try
            {
                var candidates = await _candidateServices.AdvancedSearchCandidates(searchModel);
                var paginatedCandidates = PaginatedList<Candidate>.Create(candidates.AsQueryable(), page, PAGE_SIZE);
                return Ok(paginatedCandidates);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost]
        // [Authorize(Roles = "HR Leader")]
        public async Task<IActionResult> CreateCandidate(CandidateCreateViewModel candidate)
        {
            try
            {
                await _candidateServices.CreateCandidateAsync(candidate);
                return CreatedAtAction(nameof(GetCandidateById), new { id = candidate.CandidateId }, candidate);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("GetAllCandidates")]
        // [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetCandidates(int page = 1)
        {
            try
            {
                var candidates = await _candidateServices.GetAllCandidatesAsync();
                var paginatedCandidates = PaginatedList<Candidate>.Create(candidates.AsQueryable(), page, PAGE_SIZE);
                return Ok(paginatedCandidates);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("{id}")]
        // [Authorize(Roles = "Admin, HR Leader")]
        public async Task<IActionResult> GetCandidateById(Guid id)
        {
            try
            {
                var candidate = await _candidateServices.GetCandidateByIdAsync(id);
                if (candidate == null) return NotFound();
                return Ok(candidate);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPut("{id}/status")]
        // [Authorize(Roles = "Admin, HR Leader")]
        public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] string status)
        {
            try
            {
                await _candidateServices.UpdateCandidateStatusAsync(id, status);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPut("{id}/update")]
        // [Authorize(Roles = "Admin, HR Leader")]
        public async Task<IActionResult> UpdateCandidate(Guid id, CandidateCreateViewModel updatedCandidate)
        {
            if (id != updatedCandidate.CandidateId)
            {
                return BadRequest("Candidate ID mismatch");
            }

            try
            {
                await _candidateServices.UpdateCandidateAsync(updatedCandidate);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpDelete("{id}")]
        // [Authorize(Roles = "Admin, HR Leader")]
        public async Task<IActionResult> DeleteCandidate(Guid id)
        {
            try
            {
                await _candidateServices.DeleteCandidateAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
