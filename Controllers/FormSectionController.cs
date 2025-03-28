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
    public class FormSectionController : ControllerBase
    {
        private readonly FormService _formService;
        private readonly SectionPermissionService _permissionService;
        private readonly AuditTrailService _auditTrailService;

        public FormSectionController(IConfiguration configuration)
        {
            _formService = new FormService(configuration);
            _permissionService = new SectionPermissionService(configuration);
            _auditTrailService = new AuditTrailService(configuration);
        }

        [HttpGet("form/{formId}")]
        public IActionResult GetSectionsByFormId(int formId)
        {
            try
            {
                var currentUserId = User.Identity.Name;
                var isAdmin = User.IsInRole("Admin");
                var isCommitteeMember = User.IsInRole("CommitteeMember");

                // בדיקה שהטופס קיים
                var form = _formService.GetFormById(formId);
                if (form == null)
                    return NotFound($"Form with ID {formId} not found");

                // בדיקת הרשאות
                if (!form.IsPublished && !isAdmin && !isCommitteeMember)
                    return Forbid();

                var sections = _formService.GetFormStructure(formId);
                return Ok(sections);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("{id}")]
        public IActionResult GetSectionById(int id)
        {
            try
            {
                var currentUserId = User.Identity.Name;
                var isAdmin = User.IsInRole("Admin");
                var isCommitteeMember = User.IsInRole("CommitteeMember");

                // קבלת הסעיף
                var section = _formService.GetSectionById(id);
                if (section == null)
                    return NotFound($"Section with ID {id} not found");

                // בדיקת הרשאות
                if (!isAdmin && !isCommitteeMember && !_permissionService.CanViewSection(currentUserId, id))
                    return Forbid();

                return Ok(section);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin,CommitteeMember")]
        public async Task<IActionResult> CreateSection([FromBody] FormSection section)
        {
            try
            {
                if (section == null)
                    return BadRequest("Section data is null");

                // בדיקה שהטופס קיים
                var form = _formService.GetFormById(section.FormId);
                if (form == null)
                    return NotFound($"Form with ID {section.FormId} not found");

                // אם הטופס כבר מפורסם, לא ניתן להוסיף סעיפים חדשים
                if (form.IsPublished)
                    return BadRequest("Cannot add sections to a published form");

                var sectionId = _formService.AddSection(section);
                if (sectionId > 0)
                {
                    // לוג הפעולה
                    var currentUserId = User.Identity.Name;
                    await _auditTrailService.LogActionAsync(
                        currentUserId,
                        "Create",
                        "FormSection",
                        sectionId,
                        $"Created new section: {section.Title} for form {section.FormId}"
                    );

                    section.SectionID = sectionId;
                    return CreatedAtAction(nameof(GetSectionById), new { id = sectionId }, section);
                }
                else
                {
                    return BadRequest("Failed to create section");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,CommitteeMember")]
        public async Task<IActionResult> UpdateSection(int id, [FromBody] FormSection section)
        {
            try
            {
                if (section == null)
                    return BadRequest("Section data is null");

                if (id != section.SectionID)
                    return BadRequest("ID mismatch");

                // בדיקה שהסעיף קיים
                var existingSection = _formService.GetSectionById(id);
                if (existingSection == null)
                    return NotFound($"Section with ID {id} not found");

                // בדיקה שהטופס קיים
                var form = _formService.GetFormById(existingSection.FormId);
                if (form == null)
                    return NotFound($"Form with ID {existingSection.FormId} not found");

                // אם הטופס כבר מפורסם, לא ניתן לעדכן סעיפים
                if (form.IsPublished)
                    return BadRequest("Cannot update sections in a published form");

                var result = _formService.UpdateSection(section);
                if (result > 0)
                {
                    // לוג הפעולה
                    var currentUserId = User.Identity.Name;
                    await _auditTrailService.LogActionAsync(
                        currentUserId,
                        "Update",
                        "FormSection",
                        id,
                        $"Updated section: {section.Title}"
                    );

                    return Ok(section);
                }
                else
                {
                    return BadRequest("Failed to update section");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,CommitteeMember")]
        public async Task<IActionResult> DeleteSection(int id)
        {
            try
            {
                // בדיקה שהסעיף קיים
                var section = _formService.GetSectionById(id);
                if (section == null)
                    return NotFound($"Section with ID {id} not found");

                // בדיקה שהטופס קיים
                var form = _formService.GetFormById(section.FormId);
                if (form == null)
                    return NotFound($"Form with ID {section.FormId} not found");

                // אם הטופס כבר מפורסם, לא ניתן למחוק סעיפים
                if (form.IsPublished)
                    return BadRequest("Cannot delete sections from a published form");

                var result = _formService.DeleteSection(id);
                if (result > 0)
                {
                    // לוג הפעולה
                    var currentUserId = User.Identity.Name;
                    await _auditTrailService.LogActionAsync(
                        currentUserId,
                        "Delete",
                        "FormSection",
                        id,
                        $"Deleted section: {section.Title}"
                    );

                    return Ok(new { Message = "Section deleted successfully" });
                }
                else
                {
                    return BadRequest("Failed to delete section");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("{id}/fields")]
        public IActionResult GetSectionFields(int id)
        {
            try
            {
                var currentUserId = User.Identity.Name;
                var isAdmin = User.IsInRole("Admin");
                var isCommitteeMember = User.IsInRole("CommitteeMember");

                // בדיקה שהסעיף קיים
                var section = _formService.GetSectionById(id);
                if (section == null)
                    return NotFound($"Section with ID {id} not found");

                // בדיקת הרשאות
                if (!isAdmin && !isCommitteeMember && !_permissionService.CanViewSection(currentUserId, id))
                    return Forbid();

                var fields = _formService.GetSectionFields(id);
                return Ok(fields);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("{id}/permissions")]
        [Authorize(Roles = "Admin,CommitteeMember")]
        public async Task<IActionResult> AssignSectionPermission(int id, [FromBody] SectionPermission permission)
        {
            try
            {
                if (permission == null)
                    return BadRequest("Permission data is null");

                if (id != permission.SectionID)
                    return BadRequest("Section ID mismatch");

                var result = _permissionService.AssignPermission(permission);
                if (result > 0)
                {
                    // לוג הפעולה
                    var currentUserId = User.Identity.Name;
                    await _auditTrailService.LogActionAsync(
                        currentUserId,
                        "AssignPermission",
                        "FormSection",
                        id,
                        $"Assigned permission to section for person {permission.ResponsiblePerson}"
                    );

                    return Ok(permission);
                }
                else
                {
                    return BadRequest("Failed to assign permission");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpDelete("permissions/{permissionId}")]
        [Authorize(Roles = "Admin,CommitteeMember")]
        public async Task<IActionResult> RemoveSectionPermission(int permissionId)
        {
            try
            {
                var result = _permissionService.RemovePermission(permissionId);
                if (result > 0)
                {
                    // לוג הפעולה
                    var currentUserId = User.Identity.Name;
                    await _auditTrailService.LogActionAsync(
                        currentUserId,
                        "RemovePermission",
                        "SectionPermission",
                        permissionId,
                        $"Removed section permission"
                    );

                    return Ok(new { Message = "Permission removed successfully" });
                }
                else
                {
                    return BadRequest("Failed to remove permission");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}