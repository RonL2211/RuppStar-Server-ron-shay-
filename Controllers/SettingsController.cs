using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FinalProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class SettingsController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public SettingsController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult GetSystemSettings()
        {
            try
            {
                var settings = new
                {
                    SystemName = _configuration["SystemSettings:Name"] ?? "RuppStar",
                    AcademicYear = _configuration["SystemSettings:CurrentAcademicYear"],
                    SubmissionDeadline = _configuration["SystemSettings:SubmissionDeadline"],
                    MinimumScoreForExcellence = _configuration["SystemSettings:MinimumScore"],
                    MaxAppealDays = _configuration["SystemSettings:MaxAppealDays"],
                    EmailNotificationsEnabled = bool.Parse(_configuration["SystemSettings:EmailNotificationsEnabled"] ?? "true")
                };

                return Ok(settings);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPut]
        public async Task<IActionResult> UpdateSystemSettings([FromBody] Dictionary<string, string> settings)
        {
            try
            {
                // בפרויקט אמיתי יש לממש עדכון הגדרות באופן נכון
                // לרוב נשמר בקובץ או במסד נתונים, לא בקובץ התצורה עצמו

                // לצורך הדוגמה, נחזיר הצלחה
                return Ok(new { Message = "Settings updated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("email")]
        public IActionResult GetEmailSettings()
        {
            try
            {
                var emailSettings = new
                {
                    SmtpServer = _configuration["EmailSettings:SmtpServer"],
                    SmtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"] ?? "587"),
                    FromEmail = _configuration["EmailSettings:FromEmail"],
                    FromName = _configuration["EmailSettings:FromName"],
                    UseSsl = bool.Parse(_configuration["EmailSettings:UseSsl"] ?? "true")
                };

                return Ok(emailSettings);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPut("email")]
        public async Task<IActionResult> UpdateEmailSettings([FromBody] Dictionary<string, string> emailSettings)
        {
            try
            {
                // בפרויקט אמיתי יש לממש עדכון הגדרות דוא"ל באופן נכון

                // לצורך הדוגמה, נחזיר הצלחה
                return Ok(new { Message = "Email settings updated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("academicYears")]
        public IActionResult GetAcademicYears()
        {
            try
            {
                // בדרך כלל זה יבוא ממסד הנתונים
                var academicYears = new List<string>
                {
                    "2022-2023",
                    "2023-2024",
                    "2024-2025",
                    "2025-2026"
                };

                return Ok(academicYears);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}