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
    public class FormController : ControllerBase
    {
        private readonly FormService _formService;
        private readonly FormValidationService _validationService;
        private readonly RoleService _roleService;
        private readonly AuditTrailService _auditTrailService;

        public FormController(IConfiguration configuration)
        {
            _formService = new FormService(configuration);
            _validationService = new FormValidationService(configuration);
            _roleService = new RoleService(configuration);
            _auditTrailService = new AuditTrailService(configuration);
        }

        [HttpGet]
        public IActionResult GetAllForms()
        {
            try
            {
                var currentUserId = User.Identity.Name;
                var isAdmin = User.IsInRole("Admin");
                var isCommitteeMember = _roleService.IsCommitteeMember(currentUserId);

                // אם המשתמש הוא מנהל מערכת או חבר ועדה, מחזירים את כל הטפסים
                // אחרת, מחזירים רק את הטפסים המפורסמים
                var forms = isAdmin || isCommitteeMember
                    ? _formService.GetAllForms()
                    : _formService.GetPublishedForms();

                return Ok(forms);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("{id}")]
        public IActionResult GetFormById(int id)
        {
            try
            {
                var form = _formService.GetFormById(id);
                if (form == null)
                    return NotFound($"Form with ID {id} not found");

                var currentUserId = User.Identity.Name;
                var isAdmin = User.IsInRole("Admin");
                var isCommitteeMember = _roleService.IsCommitteeMember(currentUserId);

                // אם הטופס לא מפורסם, רק מנהל מערכת או חבר ועדה יכולים לצפות בו
                if (!form.IsPublished && !isAdmin && !isCommitteeMember)
                    return Forbid();

                return Ok(form);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("academicYear/{year}")]
        public IActionResult GetFormsByAcademicYear(string year)
        {
            try
            {
                var forms = _formService.GetFormsByAcademicYear(year);

                var currentUserId = User.Identity.Name;
                var isAdmin = User.IsInRole("Admin");
                var isCommitteeMember = _roleService.IsCommitteeMember(currentUserId);

                // אם המשתמש הוא מנהל מערכת או חבר ועדה, מחזירים את כל הטפסים
                // אחרת, מחזירים רק את הטפסים המפורסמים
                if (!isAdmin && !isCommitteeMember)
                {
                    forms = forms.FindAll(f => f.IsPublished && f.IsActive);
                }

                return Ok(forms);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin,CommitteeMember")]
        public async Task<IActionResult> CreateForm([FromBody] Form form)
        {
            try
            {
                if (form == null)
                    return BadRequest("Form data is null");

                var currentUserId = User.Identity.Name;
                form.CreatedBy = currentUserId;
                form.LastModifiedBy = currentUserId;

                var formId = _formService.CreateForm(form);
                if (formId > 0)
                {
                    // לוג הפעולה
                    await _auditTrailService.LogActionAsync(
                        currentUserId,
                        "Create",
                        "Form",
                        formId,
                        $"Created new form: {form.FormName}"
                    );

                    form.FormID = formId;
                    return CreatedAtAction(nameof(GetFormById), new { id = formId }, form);
                }
                else
                {
                    return BadRequest("Failed to create form");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,CommitteeMember")]
        public async Task<IActionResult> UpdateForm(int id, [FromBody] Form form)
        {
            try
            {
                if (form == null)
                    return BadRequest("Form data is null");

                if (id != form.FormID)
                    return BadRequest("ID mismatch");

                var existingForm = _formService.GetFormById(id);
                if (existingForm == null)
                    return NotFound($"Form with ID {id} not found");

                var currentUserId = User.Identity.Name;
                form.LastModifiedBy = currentUserId;

                var result = _formService.UpdateForm(form);
                if (result > 0)
                {
                    // לוג הפעולה
                    await _auditTrailService.LogActionAsync(
                        currentUserId,
                        "Update",
                        "Form",
                        id,
                        $"Updated form: {form.FormName}"
                    );

                    return Ok(form);
                }
                else
                {
                    return BadRequest("Failed to update form");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("{id}/publish")]
        [Authorize(Roles = "Admin,CommitteeMember")]
        public async Task<IActionResult> PublishForm(int id)
        {
            try
            {
                var form = _formService.GetFormById(id);
                if (form == null)
                    return NotFound($"Form with ID {id} not found");

                // בדיקת תקינות הטופס
                var validationErrors = _validationService.ValidateFormStructure(id);
                if (validationErrors.Count > 0)
                    return BadRequest(validationErrors);

                var currentUserId = User.Identity.Name;
                var result = _formService.PublishForm(id, currentUserId);
                if (result > 0)
                {
                    // לוג הפעולה
                    await _auditTrailService.LogActionAsync(
                        currentUserId,
                        "Publish",
                        "Form",
                        id,
                        $"Published form: {form.FormName}"
                    );

                    return Ok(new { Message = "Form published successfully" });
                }
                else
                {
                    return BadRequest("Failed to publish form");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("{id}/structure")]
        public IActionResult GetFormStructure(int id)
        {
            try
            {
                var form = _formService.GetFormById(id);
                if (form == null)
                    return NotFound($"Form with ID {id} not found");

                var structure = _formService.GetFormSectionHierarchy(id);
                return Ok(structure);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("validate/{id}")]
        [Authorize(Roles = "Admin,CommitteeMember")]
        public IActionResult ValidateForm(int id)
        {
            try
            {
                var validationErrors = _validationService.ValidateFormStructure(id);
                return Ok(new { IsValid = validationErrors.Count == 0, Errors = validationErrors });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}