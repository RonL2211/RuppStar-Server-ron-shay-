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
    public class SectionFieldController : ControllerBase
    {
        private readonly FormService _formService;
        private readonly SectionPermissionService _permissionService;
        private readonly AuditTrailService _auditTrailService;

        public SectionFieldController(IConfiguration configuration)
        {
            _formService = new FormService(configuration);
            _permissionService = new SectionPermissionService(configuration);
            _auditTrailService = new AuditTrailService(configuration);
        }

        [HttpGet("{id}")]
        public IActionResult GetFieldById(int id)
        {
            try
            {
                var currentUserId = User.Identity.Name;
                var isAdmin = User.IsInRole("Admin");
                var isCommitteeMember = User.IsInRole("CommitteeMember");

                // קבלת השדה
                var field = _formService.GetFieldById(id);
                if (field == null)
                    return NotFound($"Field with ID {id} not found");

                // קבלת הסעיף שהשדה שייך אליו
                var section = _formService.GetSectionById(field.SectionID.Value);
                if (section == null)
                    return NotFound($"Section with ID {field.SectionID} not found");

                // בדיקת הרשאות
                if (!isAdmin && !isCommitteeMember && !_permissionService.CanViewSection(currentUserId, field.SectionID.Value))
                    return Forbid();

                return Ok(field);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin,CommitteeMember")]
        public async Task<IActionResult> CreateField([FromBody] SectionField field)
        {
            try
            {
                if (field == null)
                    return BadRequest("Field data is null");

                // בדיקה שהסעיף קיים
                var section = _formService.GetSectionById(field.SectionID.Value);
                if (section == null)
                    return NotFound($"Section with ID {field.SectionID} not found");

                // בדיקה שהטופס קיים
                var form = _formService.GetFormById(section.FormId);
                if (form == null)
                    return NotFound($"Form with ID {section.FormId} not found");

                // אם הטופס כבר מפורסם, לא ניתן להוסיף שדות חדשים
                if (form.IsPublished)
                    return BadRequest("Cannot add fields to a published form");

                var fieldId = _formService.AddField(field);
                if (fieldId > 0)
                {
                    // לוג הפעולה
                    var currentUserId = User.Identity.Name;
                    await _auditTrailService.LogActionAsync(
                        currentUserId,
                        "Create",
                        "SectionField",
                        fieldId,
                        $"Created new field: {field.FieldLabel} for section {field.SectionID}"
                    );

                    field.FieldID = fieldId;
                    return CreatedAtAction(nameof(GetFieldById), new { id = fieldId }, field);
                }
                else
                {
                    return BadRequest("Failed to create field");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,CommitteeMember")]
        public async Task<IActionResult> UpdateField(int id, [FromBody] SectionField field)
        {
            try
            {
                if (field == null)
                    return BadRequest("Field data is null");

                if (id != field.FieldID)
                    return BadRequest("ID mismatch");

                // בדיקה שהשדה קיים
                var existingField = _formService.GetFieldById(id);
                if (existingField == null)
                    return NotFound($"Field with ID {id} not found");

                // בדיקה שהסעיף קיים
                var section = _formService.GetSectionById(existingField.SectionID.Value);
                if (section == null)
                    return NotFound($"Section with ID {existingField.SectionID} not found");

                // בדיקה שהטופס קיים
                var form = _formService.GetFormById(section.FormId);
                if (form == null)
                    return NotFound($"Form with ID {section.FormId} not found");

                // אם הטופס כבר מפורסם, לא ניתן לעדכן שדות
                if (form.IsPublished)
                    return BadRequest("Cannot update fields in a published form");

                var result = _formService.UpdateField(field);
                if (result > 0)
                {
                    // לוג הפעולה
                    var currentUserId = User.Identity.Name;
                    await _auditTrailService.LogActionAsync(
                        currentUserId,
                        "Update",
                        "SectionField",
                        id,
                        $"Updated field: {field.FieldLabel}"
                    );

                    return Ok(field);
                }
                else
                {
                    return BadRequest("Failed to update field");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,CommitteeMember")]
        public async Task<IActionResult> DeleteField(int id)
        {
            try
            {
                // בדיקה שהשדה קיים
                var field = _formService.GetFieldById(id);
                if (field == null)
                    return NotFound($"Field with ID {id} not found");

                // בדיקה שהסעיף קיים
                var section = _formService.GetSectionById(field.SectionID.Value);
                if (section == null)
                    return NotFound($"Section with ID {field.SectionID} not found");

                // בדיקה שהטופס קיים
                var form = _formService.GetFormById(section.FormId);
                if (form == null)
                    return NotFound($"Form with ID {section.FormId} not found");

                // אם הטופס כבר מפורסם, לא ניתן למחוק שדות
                if (form.IsPublished)
                    return BadRequest("Cannot delete fields from a published form");

                var result = _formService.DeleteField(id);
                if (result > 0)
                {
                    // לוג הפעולה
                    var currentUserId = User.Identity.Name;
                    await _auditTrailService.LogActionAsync(
                        currentUserId,
                        "Delete",
                        "SectionField",
                        id,
                        $"Deleted field: {field.FieldLabel}"
                    );

                    return Ok(new { Message = "Field deleted successfully" });
                }
                else
                {
                    return BadRequest("Failed to delete field");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("{id}/options")]
        public IActionResult GetFieldOptions(int id)
        {
            try
            {
                var currentUserId = User.Identity.Name;
                var isAdmin = User.IsInRole("Admin");
                var isCommitteeMember = User.IsInRole("CommitteeMember");

                // בדיקה שהשדה קיים
                var field = _formService.GetFieldById(id);
                if (field == null)
                    return NotFound($"Field with ID {id} not found");

                // קבלת הסעיף שהשדה שייך אליו
                var section = _formService.GetSectionById(field.SectionID.Value);
                if (section == null)
                    return NotFound($"Section with ID {field.SectionID} not found");

                // בדיקת הרשאות
                if (!isAdmin && !isCommitteeMember && !_permissionService.CanViewSection(currentUserId, field.SectionID.Value))
                    return Forbid();

                var options = _formService.GetFieldOptions(id);
                return Ok(options);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("options")]
        [Authorize(Roles = "Admin,CommitteeMember")]
        public async Task<IActionResult> AddFieldOption([FromBody] FieldOption option)
        {
            try
            {
                if (option == null)
                    return BadRequest("Option data is null");

                // בדיקה שהשדה קיים
                var field = _formService.GetFieldById(option.FieldID);
                if (field == null)
                    return NotFound($"Field with ID {option.FieldID} not found");

                // בדיקה שהסעיף קיים
                var section = _formService.GetSectionById(field.SectionID.Value);
                if (section == null)
                    return NotFound($"Section with ID {field.SectionID} not found");

                // בדיקה שהטופס קיים
                var form = _formService.GetFormById(section.FormId);
                if (form == null)
                    return NotFound($"Form with ID {section.FormId} not found");

                // אם הטופס כבר מפורסם, לא ניתן להוסיף אפשרויות חדשות
                if (form.IsPublished)
                    return BadRequest("Cannot add options to a published form");

                var optionId = _formService.AddFieldOption(option);
                if (optionId > 0)
                {
                    // לוג הפעולה
                    var currentUserId = User.Identity.Name;
                    await _auditTrailService.LogActionAsync(
                        currentUserId,
                        "Create",
                        "FieldOption",
                        optionId,
                        $"Created new option: {option.OptionLabel} for field {option.FieldID}"
                    );

                    option.OptionID = optionId;
                    return CreatedAtAction(nameof(GetFieldOptions), new { id = option.FieldID }, option);
                }
                else
                {
                    return BadRequest("Failed to create option");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPut("options/{id}")]
        [Authorize(Roles = "Admin,CommitteeMember")]
        public async Task<IActionResult> UpdateFieldOption(int id, [FromBody] FieldOption option)
        {
            try
            {
                if (option == null)
                    return BadRequest("Option data is null");

                if (id != option.OptionID)
                    return BadRequest("ID mismatch");

                // בדיקה שהשדה קיים
                var field = _formService.GetFieldById(option.FieldID);
                if (field == null)
                    return NotFound($"Field with ID {option.FieldID} not found");

                // בדיקה שהסעיף קיים
                var section = _formService.GetSectionById(field.SectionID.Value);
                if (section == null)
                    return NotFound($"Section with ID {field.SectionID} not found");

                // בדיקה שהטופס קיים
                var form = _formService.GetFormById(section.FormId);
                if (form == null)
                    return NotFound($"Form with ID {section.FormId} not found");

                // אם הטופס כבר מפורסם, לא ניתן לעדכן אפשרויות
                if (form.IsPublished)
                    return BadRequest("Cannot update options in a published form");

                var result = _formService.UpdateFieldOption(option);
                if (result > 0)
                {
                    // לוג הפעולה
                    var currentUserId = User.Identity.Name;
                    await _auditTrailService.LogActionAsync(
                        currentUserId,
                        "Update",
                        "FieldOption",
                        id,
                        $"Updated option: {option.OptionLabel}"
                    );

                    return Ok(option);
                }
                else
                {
                    return BadRequest("Failed to update option");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpDelete("options/{id}")]
        [Authorize(Roles = "Admin,CommitteeMember")]
        public async Task<IActionResult> DeleteFieldOption(int id)
        {
            try
            {
                // קבלת האפשרות
                // בעיה: אין שירות לקבלת אפשרות לפי מזהה. נניח שהשירות קיים.
                var option = _formService.GetFieldOptionById(id);
                if (option == null)
                    return NotFound($"Option with ID {id} not found");

                // בדיקה שהשדה קיים
                var field = _formService.GetFieldById(option.FieldID);
                if (field == null)
                    return NotFound($"Field with ID {option.FieldID} not found");

                // בדיקה שהסעיף קיים
                var section = _formService.GetSectionById(field.SectionID.Value);
                if (section == null)
                    return NotFound($"Section with ID {field.SectionID} not found");

                // בדיקה שהטופס קיים
                var form = _formService.GetFormById(section.FormId);
                if (form == null)
                    return NotFound($"Form with ID {section.FormId} not found");

                // אם הטופס כבר מפורסם, לא ניתן למחוק אפשרויות
                if (form.IsPublished)
                    return BadRequest("Cannot delete options from a published form");

                var result = _formService.DeleteFieldOption(id);
                if (result > 0)
                {
                    // לוג הפעולה
                    var currentUserId = User.Identity.Name;
                    await _auditTrailService.LogActionAsync(
                        currentUserId,
                        "Delete",
                        "FieldOption",
                        id,
                        $"Deleted option: {option.OptionLabel}"
                    );

                    return Ok(new { Message = "Option deleted successfully" });
                }
                else
                {
                    return BadRequest("Failed to delete option");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}