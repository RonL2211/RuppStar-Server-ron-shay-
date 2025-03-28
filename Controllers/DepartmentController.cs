using FinalProject.BL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;

namespace FinalProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DepartmentController : ControllerBase
    {
        private readonly DepartmentService _departmentService;

        public DepartmentController(IConfiguration configuration)
        {
            _departmentService = new DepartmentService(configuration);
        }

        [HttpGet]
        public IActionResult GetAllDepartments()
        {
            try
            {
                var departments = _departmentService.GetAllDepartments();
                return Ok(departments);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("{id}")]
        public IActionResult GetDepartmentById(int id)
        {
            try
            {
                var department = _departmentService.GetDepartmentById(id);
                if (department == null)
                    return NotFound($"Department with ID {id} not found");

                return Ok(department);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("faculty/{facultyId}")]
        public IActionResult GetDepartmentsByFacultyId(int facultyId)
        {
            try
            {
                var departments = _departmentService.GetDepartmentsByFacultyId(facultyId);
                return Ok(departments);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("faculties")]
        public IActionResult GetAllFaculties()
        {
            try
            {
                var faculties = _departmentService.GetAllFaculties();
                return Ok(faculties);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("faculties/{id}")]
        public IActionResult GetFacultyById(int id)
        {
            try
            {
                var faculty = _departmentService.GetFacultyById(id);
                if (faculty == null)
                    return NotFound($"Faculty with ID {id} not found");

                return Ok(faculty);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}