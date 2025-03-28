using FinalProject.BL.Services;
using FinalProject.DAL.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;

namespace FinalProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FormInstanceController : ControllerBase
    {
        private readonly FormInstanceService _instanceService;
        private readonly FormService _formService;
        private readonly FormValidationService _validationService;
        private readonly NotificationService _notificationService;
        private readonly AuditTrailService _auditTrailService;

        public FormInstanceController(IConfiguration configuration)
        {
            _instanceService = new FormInstanceService(configuration);
            _formService = new FormService(configuration);
            _validationService = new FormValidationService(configuration);
            _notificationService = new NotificationService(configuration);
            _auditTrailService = new AuditTrailService(configuration);
        }

        [HttpGet("user/{userId}")]
        public IActionResult GetUserInstances(string userId)
        {
            try
            {
                var currentUserId = User.Identity.Name;
                var isAdmin = User.IsInRole("Admin");
                var isFacultyHead = User.IsInRole("FacultyHead");
                var isCommitteeMember = User.IsInRole("CommitteeMember");

                // בדיקת הרשאות - משתמש רגיל יכול לראות רק את המופעים שלו
                if (currentUserId != userId && !isAdmin && !isFacultyHead && !isCommitteeMember)
                    return Forbid();

                var instances = _instanceService.GetUserInstances(userId);
                return Ok(instances);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("{id}")]
        public IActionResult GetInstanceById(int id)
        {
            try
            {
                var instance = _instanceService.GetInstanceById(id);
                if (instance == null)
                    return NotFound($"Instance with ID {id} not found");

                var currentUserId = User.Identity.Name;
                var isAdmin = User.IsInRole("Admin");
                var isFacultyHead = User.IsInRole("FacultyHead");
                var isCommitteeMember = User.IsInRole("CommitteeMember");

                // בדיקת הרשאות - משתמש רגיל יכול לראות רק את המופעים שלו
                if (currentUserId != instance.UserID && !isAdmin && !isFacultyHead && !isCommitteeMember)
                    return Forbid();

                return Ok(instance);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("form/{formId}")]
        public async Task<IActionResult> CreateInstance(int formId)
        {
            try
            {
                var currentUserId = User.Identity.Name;

                // בדיקה שהטופס קיים ומפורסם
                var form = _formService.GetFormById(formId);
                if (form == null)
                    return NotFound($"Form with ID {formId} not found");

                if (!form.IsPublished)
                    return BadRequest("Cannot create instance for unpublished form");

                if (!form.IsActive)
                    return BadRequest("Cannot create instance for inactive form");

                var instanceId = _instanceService.CreateInstance(currentUserId, formId);
                if (instanceId > 0)
                {
                    // לוג הפעולה
                    await _auditTrailService.LogActionAsync(
                        currentUserId,
                        "Create",
                        "FormInstance",
                        instanceId,
                        $"Created new instance for form {formId}"
                    );

                    var instance = _instanceService.GetInstanceById(instanceId);
                    return CreatedAtAction(nameof(GetInstanceById), new { id = instanceId }, instance);
                }
                else
                {
                    return BadRequest("Failed to create instance");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("{id}/submit")]
        public async Task<IActionResult> SubmitInstance(int id, [FromBody] SubmitInstanceModel model)
        {
            try
            {
                var instance = _instanceService.GetInstanceById(id);
                if (instance == null)
                    return NotFound($"Instance with ID {id} not found");

                var currentUserId = User.Identity.Name;

                // בדיקת הרשאות - רק המשתמש שיצר את המופע יכול להגיש אותו
                if (currentUserId != instance.UserID)
                    return Forbid();

                // בדיקת תקינות המופע לפני הגשה
                var validationErrors = _validationService.ValidateFormInstance(id);
                if (validationErrors.Count > 0)
                    return BadRequest(validationErrors);

                var comments = model?.Comments;
                var result = _instanceService.SubmitInstance(id, comments);
                if (result > 0)
                {
                    // לוג הפעולה
                    await _auditTrailService.LogActionAsync(
                        currentUserId,
                        "Submit",
                        "FormInstance",
                        id,
                        $"Submitted instance {id}"
                    );

                    // שליחת התראה
                    await _notificationService.SendFormSubmissionNotificationAsync(id);

                    return Ok(new { Message = "Instance submitted successfully" });
                }
                else
                {
                    return BadRequest("Failed to submit instance");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("{id}/approve")]
        [Authorize(Roles = "Admin,FacultyHead,CommitteeMember")]
        public async Task<IActionResult> ApproveInstance(int id, [FromBody] ApproveInstanceModel model)
        {
            try
            {
                if (model == null || !model.TotalScore.HasValue)
                    return BadRequest("Total score is required");

                var instance = _instanceService.GetInstanceById(id);
                if (instance == null)
                    return NotFound($"Instance with ID {id} not found");

                var oldStatus = instance.CurrentStage;
                var result = _instanceService.ApproveInstance(id, model.TotalScore.Value, model.Comments);
                if (result > 0)
                {
                    // לוג הפעולה
                    var currentUserId = User.Identity.Name;
                    await _auditTrailService.LogActionAsync(
                        currentUserId,
                        "Approve",
                        "FormInstance",
                        id,
                        $"Approved instance {id} with score {model.TotalScore}"
                    );

                    // שליחת התראה
                    await _notificationService.SendStatusChangeNotificationAsync(id, oldStatus, "Approved");

                    return Ok(new { Message = "Instance approved successfully" });
                }
                else
                {
                    return BadRequest("Failed to approve instance");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("{id}/reject")]
        [Authorize(Roles = "Admin,FacultyHead,CommitteeMember")]
        public async Task<IActionResult> RejectInstance(int id, [FromBody] RejectInstanceModel model)
        {
            try
            {
                if (model == null || string.IsNullOrEmpty(model.Comments))
                    return BadRequest("Comments are required for rejection");

                var instance = _instanceService.GetInstanceById(id);
                if (instance == null)
                    return NotFound($"Instance with ID {id} not found");

                var oldStatus = instance.CurrentStage;
                var result = _instanceService.RejectInstance(id, model.Comments);
                if (result > 0)
                {
                    // לוג הפעולה
                    var currentUserId = User.Identity.Name;
                    await _auditTrailService.LogActionAsync(
                        currentUserId,
                        "Reject",
                        "FormInstance",
                        id,
                        $"Rejected instance {id}"
                    );

                    // שליחת התראה
                    await _notificationService.SendStatusChangeNotificationAsync(id, oldStatus, "Rejected");

                    return Ok(new { Message = "Instance rejected successfully" });
                }
                else
                {
                    return BadRequest("Failed to reject instance");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("{id}/return")]
        [Authorize(Roles = "Admin,FacultyHead,CommitteeMember")]
        public async Task<IActionResult> ReturnForRevision(int id, [FromBody] ReturnInstanceModel model)
        {
            try
            {
                if (model == null || string.IsNullOrEmpty(model.Comments))
                    return BadRequest("Comments are required for return");

                var instance = _instanceService.GetInstanceById(id);
                if (instance == null)
                    return NotFound($"Instance with ID {id} not found");

                var oldStatus = instance.CurrentStage;
                var result = _instanceService.ReturnForRevision(id, model.Comments);
                if (result > 0)
                {
                    // לוג הפעולה
                    var currentUserId = User.Identity.Name;
                    await _auditTrailService.LogActionAsync(
                        currentUserId,
                        "ReturnForRevision",
                        "FormInstance",
                        id,
                        $"Returned instance {id} for revision"
                    );

                    // שליחת התראה
                    await _notificationService.SendStatusChangeNotificationAsync(id, oldStatus, "ReturnedForRevision");

                    return Ok(new { Message = "Instance returned for revision successfully" });
                }
                else
                {
                    return BadRequest("Failed to return instance for revision");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("{id}/review")]
        [Authorize(Roles = "Admin,FacultyHead,CommitteeMember")]
        public async Task<IActionResult> MarkAsUnderReview(int id, [FromBody] ReviewInstanceModel model)
        {
            try
            {
                var instance = _instanceService.GetInstanceById(id);
                if (instance == null)
                    return NotFound($"Instance with ID {id} not found");

                var oldStatus = instance.CurrentStage;
                var comments = model?.Comments;
                var result = _instanceService.MarkAsUnderReview(id, comments);
                if (result > 0)
                {
                    // לוג הפעולה
                    var currentUserId = User.Identity.Name;
                    await _auditTrailService.LogActionAsync(
                        currentUserId,
                        "MarkAsUnderReview",
                        "FormInstance",
                        id,
                        $"Marked instance {id} as under review"
                    );

                    // שליחת התראה
                    await _notificationService.SendStatusChangeNotificationAsync(id, oldStatus, "UnderReview");

                    return Ok(new { Message = "Instance marked as under review successfully" });
                }
                else
                {
                    return BadRequest("Failed to mark instance as under review");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("{id}/appeal")]
        public async Task<IActionResult> AppealInstance(int id, [FromBody] AppealInstanceModel model)
        {
            try
            {
                if (model == null || string.IsNullOrEmpty(model.AppealReason))
                    return BadRequest("Appeal reason is required");

                var instance = _instanceService.GetInstanceById(id);
                if (instance == null)
                    return NotFound($"Instance with ID {id} not found");

                var currentUserId = User.Identity.Name;

                // בדיקת הרשאות - רק המשתמש שיצר את המופע יכול לערער עליו
                if (currentUserId != instance.UserID)
                    return Forbid();

                var oldStatus = instance.CurrentStage;
                var result = _instanceService.AppealInstance(id, model.AppealReason);
                if (result > 0)
                {
                    // לוג הפעולה
                    await _auditTrailService.LogActionAsync(
                        currentUserId,
                        "Appeal",
                        "FormInstance",
                        id,
                        $"Appealed instance {id}"
                    );

                    // שליחת התראה
                    await _notificationService.SendStatusChangeNotificationAsync(id, oldStatus, "UnderAppeal");

                    return Ok(new { Message = "Instance appealed successfully" });
                }
                else
                {
                    return BadRequest("Failed to appeal instance");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("form/{formId}")]
        [Authorize(Roles = "Admin,FacultyHead,CommitteeMember")]
        public IActionResult GetInstancesByFormId(int formId)
        {
            try
            {
                // בדיקה שהטופס קיים
                var form = _formService.GetFormById(formId);
                if (form == null)
                    return NotFound($"Form with ID {formId} not found");

                var instances = _instanceService.GetInstancesByFormId(formId);
                return Ok(instances);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("stage/{stage}")]
        [Authorize(Roles = "Admin,FacultyHead,CommitteeMember")]
        public IActionResult GetInstancesByStage(string stage)
        {
            try
            {
                var instances = _instanceService.GetInstancesByStage(stage);
                return Ok(instances);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("form/{formId}/stats")]
        [Authorize(Roles = "Admin,FacultyHead,CommitteeMember")]
        public IActionResult GetInstancesStatisticsByForm(int formId)
        {
            try
            {
                // בדיקה שהטופס קיים
                var form = _formService.GetFormById(formId);
                if (form == null)
                    return NotFound($"Form with ID {formId} not found");

                var stats = _instanceService.GetInstancesStatisticsByForm(formId);
                return Ok(stats);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("{id}/validate")]
        public IActionResult ValidateInstance(int id)
        {
            try
            {
                var instance = _instanceService.GetInstanceById(id);
                if (instance == null)
                    return NotFound($"Instance with ID {id} not found");

                var currentUserId = User.Identity.Name;

                // בדיקת הרשאות - רק המשתמש שיצר את המופע או בעל תפקיד מתאים יכול לבדוק אותו
                var isAdmin = User.IsInRole("Admin");
                var isFacultyHead = User.IsInRole("FacultyHead");
                var isCommitteeMember = User.IsInRole("CommitteeMember");

                if (currentUserId != instance.UserID && !isAdmin && !isFacultyHead && !isCommitteeMember)
                    return Forbid();

                var validationErrors = _validationService.ValidateFormInstance(id);
                return Ok(new { IsValid = validationErrors.Count == 0, Errors = validationErrors });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }

    public class SubmitInstanceModel
    {
        public string Comments { get; set; }
    }

    public class ApproveInstanceModel
    {
        public decimal? TotalScore { get; set; }
        public string Comments { get; set; }
    }

    public class RejectInstanceModel
    {
        public string Comments { get; set; }
    }

    public class ReturnInstanceModel
    {
        public string Comments { get; set; }
    }

    public class ReviewInstanceModel
    {
        public string Comments { get; set; }
    }

    public class AppealInstanceModel
    {
        public string AppealReason { get; set; }
    }
}
