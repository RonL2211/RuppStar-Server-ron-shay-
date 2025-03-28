using FinalProject.DAL.Models;
using FinalProject.DAL.Repositories;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FinalProject.BL.Services
{
    public class StatisticsService
    {
        private readonly FormInstanceRepository _instanceRepository;
        private readonly FormRepository _formRepository;
        private readonly DepartmentRepository _departmentRepository;
        private readonly PersonRepository _personRepository;

        public StatisticsService(IConfiguration configuration)
        {
            _instanceRepository = new FormInstanceRepository(configuration);
            _formRepository = new FormRepository(configuration);
            _departmentRepository = new DepartmentRepository(configuration);
            _personRepository = new PersonRepository(configuration);
        }

        public Dictionary<string, int> GetFormSubmissionStats(int formId)
        {
            if (formId <= 0)
                throw new ArgumentException("Form ID must be greater than zero");

            var instances = _instanceRepository.GetInstancesByFormId(formId);
            var result = new Dictionary<string, int>();

            // סטטיסטיקה לפי שלבים
            var stageGroups = instances.GroupBy(i => i.CurrentStage).ToList();
            foreach (var group in stageGroups)
            {
                result.Add(group.Key, group.Count());
            }

            // כמות כוללת
            result.Add("Total", instances.Count);

            return result;
        }

        public Dictionary<string, Dictionary<string, int>> GetFormSubmissionStatsByDepartment(int formId)
        {
            if (formId <= 0)
                throw new ArgumentException("Form ID must be greater than zero");

            var instances = _instanceRepository.GetInstancesByFormId(formId);
            var departments = _departmentRepository.GetAllDepartments();
            var result = new Dictionary<string, Dictionary<string, int>>();

            // סטטיסטיקה לפי מחלקות
            foreach (var department in departments)
            {
                // קבלת כל המשתמשים במחלקה
                var departmentUsers = _personRepository.GetAllPersons().Where(p => p.DepartmentID == department.DepartmentID).ToList();
                var departmentUserIds = departmentUsers.Select(p => p.PersonId).ToList();

                // קבלת כל המופעים של משתמשים במחלקה
                var departmentInstances = instances.Where(i => departmentUserIds.Contains(i.UserID)).ToList();

                // סטטיסטיקה לפי שלבים במחלקה
                var departmentStats = new Dictionary<string, int>();
                var stageGroups = departmentInstances.GroupBy(i => i.CurrentStage).ToList();
                foreach (var group in stageGroups)
                {
                    departmentStats.Add(group.Key, group.Count());
                }

                // כמות כוללת במחלקה
                departmentStats.Add("Total", departmentInstances.Count);

                result.Add(department.DepartmentName, departmentStats);
            }

            return result;
        }

        public Dictionary<string, int> GetUserSubmissionStats(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentException("User ID cannot be empty");

            var instances = _instanceRepository.GetInstancesByUserId(userId);
            var result = new Dictionary<string, int>();

            // סטטיסטיקה לפי שלבים
            var stageGroups = instances.GroupBy(i => i.CurrentStage).ToList();
            foreach (var group in stageGroups)
            {
                result.Add(group.Key, group.Count());
            }

            // כמות כוללת
            result.Add("Total", instances.Count);

            return result;
        }

        public Dictionary<string, double> GetAverageScoresByFormAndDepartment(int formId)
        {
            if (formId <= 0)
                throw new ArgumentException("Form ID must be greater than zero");

            var instances = _instanceRepository.GetInstancesByFormId(formId)
                .Where(i => i.CurrentStage == "Approved" && i.TotalScore.HasValue)
                .ToList();
            var departments = _departmentRepository.GetAllDepartments();
            var result = new Dictionary<string, double>();

            // ממוצע ציונים כללי
            if (instances.Any())
            {
                result.Add("OverallAverage", (double)instances.Average(i => i.TotalScore.Value));
            }

            // ממוצע ציונים לפי מחלקות
            foreach (var department in departments)
            {
                // קבלת כל המשתמשים במחלקה
                var departmentUsers = _personRepository.GetAllPersons().Where(p => p.DepartmentID == department.DepartmentID).ToList();
                var departmentUserIds = departmentUsers.Select(p => p.PersonId).ToList();

                // קבלת כל המופעים המאושרים של משתמשים במחלקה
                var departmentInstances = instances.Where(i => departmentUserIds.Contains(i.UserID)).ToList();

                // חישוב ממוצע למחלקה
                if (departmentInstances.Any())
                {
                    result.Add(department.DepartmentName, (double)departmentInstances.Average(i => i.TotalScore.Value));
                }
            }

            return result;
        }

        public Dictionary<string, int> GetYearlySubmissionTrends(string academicYear)
        {
            if (string.IsNullOrEmpty(academicYear))
                throw new ArgumentException("Academic year cannot be empty");

            var forms = _formRepository.GetFormsByAcademicYear(academicYear);
            var result = new Dictionary<string, int>();

            foreach (var form in forms)
            {
                var formInstances = _instanceRepository.GetInstancesByFormId(form.FormID);
                result.Add(form.FormName, formInstances.Count);
            }

            return result;
        }
    }
}