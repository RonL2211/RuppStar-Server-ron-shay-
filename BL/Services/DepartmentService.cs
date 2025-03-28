using FinalProject.DAL.Models;
using FinalProject.DAL.Repositories;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;

namespace FinalProject.BL.Services
{
    public class DepartmentService
    {
        private readonly DepartmentRepository _departmentRepository;

        public DepartmentService(IConfiguration configuration)
        {
            _departmentRepository = new DepartmentRepository(configuration);
        }

        public List<Department> GetAllDepartments()
        {
            return _departmentRepository.GetAllDepartments();
        }

        public Department GetDepartmentById(int departmentId)
        {
            if (departmentId <= 0)
                throw new ArgumentException("Department ID must be greater than zero");

            return _departmentRepository.GetDepartmentById(departmentId);
        }

        public List<Department> GetDepartmentsByFacultyId(int facultyId)
        {
            if (facultyId <= 0)
                throw new ArgumentException("Faculty ID must be greater than zero");

            return _departmentRepository.GetDepartmentsByFacultyId(facultyId);
        }

        public List<Faculty> GetAllFaculties()
        {
            return _departmentRepository.GetAllFaculties();
        }

        public Faculty GetFacultyById(int facultyId)
        {
            if (facultyId <= 0)
                throw new ArgumentException("Faculty ID must be greater than zero");

            return _departmentRepository.GetFacultyById(facultyId);
        }
    }
}