using FinalProject.DAL.Models;
using FinalProject.DAL.Repositories;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FinalProject.BL.Services
{
    public class SectionPermissionService
    {
        private readonly PersonRepository _personRepository;
        private readonly FormSectionRepository _sectionRepository;
        private readonly IConfiguration _configuration;

        public SectionPermissionService(IConfiguration configuration)
        {
            _personRepository = new PersonRepository(configuration);
            _sectionRepository = new FormSectionRepository(configuration);
            _configuration = configuration;
        }

        public bool CanViewSection(string personId, int sectionId)
        {
            if (string.IsNullOrEmpty(personId) || sectionId <= 0)
                return false;

            // בדיקת הרשאות בסעיף
            var section = _sectionRepository.GetSectionById(sectionId);
            if (section == null)
                return false;

            // אם המשתמש הוא האחראי על הסעיף, יש לו הרשאת צפייה
            if (section.ResponsiblePerson == personId)
                return true;

            // בדיקת הרשאות בטבלת הרשאות הסעיפים
            var permissionRepository = new SectionPermissionRepository(_configuration);
            var permissions = permissionRepository.GetSectionPermissions(sectionId);
            var userPermission = permissions.FirstOrDefault(p => p.ResponsiblePerson == personId);

            return userPermission != null && userPermission.CanView;
        }

        public bool CanEditSection(string personId, int sectionId)
        {
            if (string.IsNullOrEmpty(personId) || sectionId <= 0)
                return false;

            // בדיקת הרשאות בסעיף
            var section = _sectionRepository.GetSectionById(sectionId);
            if (section == null)
                return false;

            // אם המשתמש הוא האחראי על הסעיף, יש לו הרשאת עריכה
            if (section.ResponsiblePerson == personId)
                return true;

            // בדיקת הרשאות בטבלת הרשאות הסעיפים
            var permissionRepository = new SectionPermissionRepository(_configuration);
            var permissions = permissionRepository.GetSectionPermissions(sectionId);
            var userPermission = permissions.FirstOrDefault(p => p.ResponsiblePerson == personId);

            return userPermission != null && userPermission.CanEdit;
        }

        public bool CanEvaluateSection(string personId, int sectionId)
        {
            if (string.IsNullOrEmpty(personId) || sectionId <= 0)
                return false;

            // בדיקת הרשאות בסעיף
            var section = _sectionRepository.GetSectionById(sectionId);
            if (section == null)
                return false;

            // אם המשתמש הוא האחראי על הסעיף, יש לו הרשאת הערכה
            if (section.ResponsiblePerson == personId)
                return true;

            // בדיקת הרשאות בטבלת הרשאות הסעיפים
            var permissionRepository = new SectionPermissionRepository(_configuration);
            var permissions = permissionRepository.GetSectionPermissions(sectionId);
            var userPermission = permissions.FirstOrDefault(p => p.ResponsiblePerson == personId);

            return userPermission != null && userPermission.CanEvaluate;
        }

        public int AssignPermission(SectionPermission permission)
        {
            if (permission == null)
                throw new ArgumentNullException(nameof(permission), "Permission cannot be null");

            if (permission.SectionID <= 0)
                throw new ArgumentException("Section ID must be greater than zero");

            if (string.IsNullOrEmpty(permission.ResponsiblePerson))
                throw new ArgumentException("Responsible person ID cannot be empty");

            // בדיקה שהמשתמש קיים
            var person = _personRepository.GetPersonById(permission.ResponsiblePerson);
            if (person == null)
                throw new ArgumentException($"Person with ID {permission.ResponsiblePerson} does not exist");

            // בדיקה שהסעיף קיים
            var section = _sectionRepository.GetSectionById(permission.SectionID);
            if (section == null)
                throw new ArgumentException($"Section with ID {permission.SectionID} does not exist");

            // שמירת ההרשאה
            var permissionRepository = new SectionPermissionRepository(_configuration);
            return permissionRepository.AddSectionPermission(permission);
        }

        public int UpdatePermission(SectionPermission permission)
        {
            if (permission == null)
                throw new ArgumentNullException(nameof(permission), "Permission cannot be null");

            if (permission.PermissionId <= 0)
                throw new ArgumentException("Permission ID must be greater than zero");

            // בדיקה שההרשאה קיימת
            var permissionRepository = new SectionPermissionRepository(_configuration);
            var existingPermission = permissionRepository.GetPermissionById(permission.PermissionId);
            if (existingPermission == null)
                throw new ArgumentException($"Permission with ID {permission.PermissionId} does not exist");

            // עדכון ההרשאה
            return permissionRepository.UpdateSectionPermission(permission);
        }

        public int RemovePermission(int permissionId)
        {
            if (permissionId <= 0)
                throw new ArgumentException("Permission ID must be greater than zero");

            // בדיקה שההרשאה קיימת
            var permissionRepository = new SectionPermissionRepository(_configuration);
            var existingPermission = permissionRepository.GetPermissionById(permissionId);
            if (existingPermission == null)
                throw new ArgumentException($"Permission with ID {permissionId} does not exist");

            // מחיקת ההרשאה
            return permissionRepository.DeleteSectionPermission(permissionId);
        }

        public List<SectionPermission> GetSectionPermissions(int sectionId)
        {
            if (sectionId <= 0)
                throw new ArgumentException("Section ID must be greater than zero");

            var permissionRepository = new SectionPermissionRepository(_configuration);
            return permissionRepository.GetSectionPermissions(sectionId);
        }
    }
}