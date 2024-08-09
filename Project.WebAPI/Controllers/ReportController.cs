using Microsoft.AspNetCore.Mvc;
using Project.Business.Model;
using Project.Business.Service.Report;
using System;
using System.Threading.Tasks;

namespace Project.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportController : ControllerBase
    {
        private readonly IReportService _reportService;

        public ReportController(IReportService reportService)
        {
            _reportService = reportService;
        }

        [HttpPost("candidateStatistics")]
        public async Task<IActionResult> GenerateCandidateStatistics([FromBody] CandidateReportCriteria criteria)
        {
            if (criteria == null)
            {
                return BadRequest("Criteria must be provided.");
            }

            try
            {
                var result = await _reportService.GenerateCandidateStatisticsAsync(criteria);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("eventStatistics")]
        public async Task<IActionResult> GenerateEventStatistics([FromBody] EventReportCriteria criteria)
        {
            if (criteria == null)
            {
                return BadRequest("Criteria must be provided.");
            }

            try
            {
                var result = await _reportService.GenerateEventStatisticsAsync(criteria);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
    