using FinalProject.DAL.Models;
using FinalProject.DAL.Repositories;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FinalProject.BL.Services
{
    public class ReportService
    {
        private readonly FormRepository _formRepository;
        private readonly FormInstanceRepository _instanceRepository;
        private readonly PersonRepository _personRepository;
        private readonly DepartmentRepository _departmentRepository;

        public ReportService(IConfiguration configuration)
        {
            _formRepository = new FormRepository(configuration);
            _instanceRepository = new FormInstanceRepository(configuration);
            _personRepository = new PersonRepository(configuration);
            _departmentRepository = new DepartmentRepository(configuration);
        }

        public Dictionary<string, int> GetStatusDistributionReport(int formId)
        {
            if (formId <= 0)
                throw new ArgumentException("Form ID must be greater than zero");

            var instances = _instanceRepository.GetInstancesByFormId(formId);
            var report = new Dictionary<string, int>();

            // חלוקה לפי סטטוס
            var statusGroups = instances.GroupBy(i => i.CurrentStage).ToList();
            foreach (var group in statusGroups)
            {
                report.Add(group.Key, group.Count());
            }

            return report;
        }

        public Dictionary<string, Dictionary<string, int>> GetDepartmentStatusReport(int formId)
        {
            if (formId <= 0)
                throw new ArgumentException("Form ID must be greater than zero");

            var instances = _instanceRepository.GetInstancesByFormId(formId);
            var departments = _departmentRepository.GetAllDepartments();
            var report = new Dictionary<string, Dictionary<string, int>>();

            // חלוקה לפי מחלקות וסטטוס
            foreach (var department in departments)
            {
                var departmentStats = new Dictionary<string, int>();

                // קבלת המשתמשים במחלקה
                var departmentUsers = _personRepository.GetAllPersons()
                    .Where(p => p.DepartmentID == department.DepartmentID)
                    .ToList();
                var departmentUserIds = departmentUsers.Select(p => p.PersonId).ToList();

                // קבלת המופעים של משתמשים במחלקה
                var departmentInstances = instances
                    .Where(i => departmentUserIds.Contains(i.UserID))
                    .ToList();

                // חלוקה לפי סטטוס
                var statusGroups = departmentInstances.GroupBy(i => i.CurrentStage).ToList();
                foreach (var group in statusGroups)
                {
                    departmentStats.Add(group.Key, group.Count());
                }

                // הוספה לדוח הכללי
                report.Add(department.DepartmentName, departmentStats);
            }

            return report;
        }

        public List<Dictionary<string, object>> GetTopPerformersReport(int formId, int count = 10)
        {
            if (formId <= 0)
                throw new ArgumentException("Form ID must be greater than zero");

            if (count <= 0)
                throw new ArgumentException("Count must be greater than zero");

            var instances = _instanceRepository.GetInstancesByFormId(formId)
                .Where(i => i.CurrentStage == "Approved" && i.TotalScore.HasValue)
                .OrderByDescending(i => i.TotalScore.Value)
                .Take(count)
                .ToList();

            var report = new List<Dictionary<string, object>>();

            foreach (var instance in instances)
            {
                var person = _personRepository.GetPersonById(instance.UserID);
                if (person != null)
                {
                    var department = person.DepartmentID.HasValue ? _departmentRepository.GetDepartmentById(person.DepartmentID.Value) : null;

                    var entry = new Dictionary<string, object>
                    {
                        { "PersonId", person.PersonId },
                        { "Name", $"{person.FirstName} {person.LastName}" },
                        { "Department", department?.DepartmentName ?? "Unknown" },
                        { "Score", instance.TotalScore.Value },
                        { "SubmissionDate", instance.SubmissionDate },
                        { "ApprovalDate", instance.LastModifiedDate }
                    };

                    report.Add(entry);
                }
            }

            return report;
        }

        public Dictionary<string, Dictionary<string, double>> GetYearlyTrendReport(string academicYear)
        {
            if (string.IsNullOrEmpty(academicYear))
                throw new ArgumentException("Academic year cannot be empty");

            var forms = _formRepository.GetFormsByAcademicYear(academicYear);
            var report = new Dictionary<string, Dictionary<string, double>>();

            foreach (var form in forms)
            {
                var formStats = new Dictionary<string, double>();
                var instances = _instanceRepository.GetInstancesByFormId(form.FormID);

                // סך הכל הגשות
                formStats.Add("TotalSubmissions", instances.Count);

                // אחוז הגשות שאושרו
                var approvedCount = instances.Count(i => i.CurrentStage == "Approved");
                formStats.Add("ApprovalRate", instances.Count > 0 ? (double)approvedCount / instances.Count * 100 : 0);

                // ציון ממוצע
                var approvedInstances = instances.Where(i => i.CurrentStage == "Approved" && i.TotalScore.HasValue).ToList();
                formStats.Add("AverageScore", approvedInstances.Any() ? (double)approvedInstances.Average(i => i.TotalScore.Value) : 0);

                // זמן ממוצע לאישור (בימים)
                var instancesWithDates = instances
                    .Where(i => i.SubmissionDate.HasValue && i.LastModifiedDate.HasValue && i.CurrentStage == "Approved")
                    .ToList();

                var avgDays = instancesWithDates.Any()
                    ? instancesWithDates.Average(i => (i.LastModifiedDate.Value - i.SubmissionDate.Value).TotalDays)
                    : 0;

                formStats.Add("AverageApprovalTime", avgDays);

                report.Add(form.FormName, formStats);
            }

            return report;
        }

        public List<Dictionary<string, object>> GetExcellenceReportByDepartment(int formId)
        {
            if (formId <= 0)
                throw new ArgumentException("Form ID must be greater than zero");

            var instances = _instanceRepository.GetInstancesByFormId(formId)
                .Where(i => i.CurrentStage == "Approved" && i.TotalScore.HasValue)
                .ToList();

            var departments = _departmentRepository.GetAllDepartments();
            var report = new List<Dictionary<string, object>>();

            foreach (var department in departments)
            {
                // קבלת המשתמשים במחלקה
                var departmentUsers = _personRepository.GetAllPersons()
                    .Where(p => p.DepartmentID == department.DepartmentID)
                    .ToList();
                var departmentUserIds = departmentUsers.Select(p => p.PersonId).ToList();

                // קבלת המופעים המאושרים של משתמשים במחלקה
                var departmentInstances = instances
                    .Where(i => departmentUserIds.Contains(i.UserID))
                    .ToList();

                if (departmentInstances.Any())
                {
                    // חישוב הציון הממוצע
                    var avgScore = departmentInstances.Average(i => i.TotalScore.Value);

                    // מציאת המרצה המצטיין במחלקה
                    var topInstance = departmentInstances.OrderByDescending(i => i.TotalScore.Value).First();
                    var topPerson = _personRepository.GetPersonById(topInstance.UserID);

                    var entry = new Dictionary<string, object>
                    {
                        { "DepartmentId", department.DepartmentID },
                        { "DepartmentName", department.DepartmentName },
                        { "SubmissionCount", departmentInstances.Count },
                        { "AverageScore", avgScore },
                        { "TopInstructorId", topPerson.PersonId },
                        { "TopInstructorName", $"{topPerson.FirstName} {topPerson.LastName}" },
                        { "TopScore", topInstance.TotalScore.Value }
                    };

                    report.Add(entry);
                }
            }

            return report;
        }
    }
}