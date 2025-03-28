using FinalProject.BL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;

namespace FinalProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,CommitteeMember")]
    public class ReportController : ControllerBase
    {
        private readonly ReportService _reportService;
        private readonly StatisticsService _statisticsService;

        public ReportController(IConfiguration configuration)
        {
            _reportService = new ReportService(configuration);
            _statisticsService = new StatisticsService(configuration);
        }

        [HttpGet("form/{formId}/status")]
        public IActionResult GetStatusDistributionReport(int formId)
        {
            try
            {
                var report = _reportService.GetStatusDistributionReport(formId);
                return Ok(report);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("form/{formId}/departments")]
        public IActionResult GetDepartmentStatusReport(int formId)
        {
            try
            {
                var report = _reportService.GetDepartmentStatusReport(formId);
                return Ok(report);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("form/{formId}/topperformers")]
        public IActionResult GetTopPerformersReport(int formId, [FromQuery] int count = 10)
        {
            try
            {
                var report = _reportService.GetTopPerformersReport(formId, count);
                return Ok(report);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("academicYear/{year}/trends")]
        public IActionResult GetYearlyTrendReport(string year)
        {
            try
            {
                var report = _reportService.GetYearlyTrendReport(year);
                return Ok(report);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("form/{formId}/excellence")]
        public IActionResult GetExcellenceReportByDepartment(int formId)
        {
            try
            {
                var report = _reportService.GetExcellenceReportByDepartment(formId);
                return Ok(report);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("form/{formId}/scores")]
        public IActionResult GetAverageScoresByFormAndDepartment(int formId)
        {
            try
            {
                var report = _statisticsService.GetAverageScoresByFormAndDepartment(formId);
                return Ok(report);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("academicYear/{year}/submissions")]
        public IActionResult GetYearlySubmissionTrends(string year)
        {
            try
            {
                var report = _statisticsService.GetYearlySubmissionTrends(year);
                return Ok(report);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("user/{userId}/stats")]
        public IActionResult GetUserSubmissionStats(string userId)
        {
            try
            {
                var report = _statisticsService.GetUserSubmissionStats(userId);
                return Ok(report);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}