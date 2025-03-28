using FinalProject.BL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;

namespace FinalProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AuditTrailController : ControllerBase
    {
        private readonly AuditTrailService _auditTrailService;

        public AuditTrailController(IConfiguration configuration)
        {
            _auditTrailService = new AuditTrailService(configuration);
        }

        [HttpGet]
        public async Task<IActionResult> GetAuditTrail(
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null,
            [FromQuery] string userId = null,
            [FromQuery] string action = null,
            [FromQuery] string entityType = null)
        {
            try
            {
                var auditTrail = await _auditTrailService.GetAuditTrailAsync(fromDate, toDate, userId, action, entityType);
                return Ok(auditTrail);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserActions(string userId)
        {
            try
            {
                var userActions = await _auditTrailService.GetUserActionsAsync(userId);
                return Ok(userActions);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("entity/{entityType}/{entityId}")]
        public async Task<IActionResult> GetEntityHistory(string entityType, int entityId)
        {
            try
            {
                var entityHistory = await _auditTrailService.GetEntityHistoryAsync(entityType, entityId);
                return Ok(entityHistory);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}