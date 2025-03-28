using FinalProject.DAL.Models;
using FinalProject.DAL.Repositories;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace FinalProject.BL.Services
{
    public class AuthenticationService
    {
        private readonly PersonRepository _personRepository;
        private readonly RoleService _roleService;

        public AuthenticationService(IConfiguration configuration)
        {
            _personRepository = new PersonRepository(configuration);
            _roleService = new RoleService(configuration);
        }

        public Person Authenticate(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                return null;

            var person = _personRepository.GetPersonByUsername(username);
            if (person == null)
                return null;

            if (!VerifyPassword(password, person.Password))
                return null;

            // לא להחזיר את הסיסמה למשתמש
            person.Password = null;
            return person;
        }

        private bool VerifyPassword(string password, string hashedPassword)
        {
            // פונקצית השוואת סיסמאות - במערכת אמיתית תשתמש בספריית הצפנה מתאימה
            // כאן מניחים שהסיסמה מאוחסנת כבר בפורמט מוצפן
            string computedHash = HashPassword(password);
            return hashedPassword.Equals(computedHash);
        }

        private string HashPassword(string password)
        {
            // פונקציית עזר להצפנת סיסמה
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(password));

                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        public bool IsAuthorizedForEntity(string personId, int entityId, string requiredRole)
        {
            // בדיקה אם למשתמש יש תפקיד מתאים (כמו ראש פקולטה) לגישה לישות מסוימת (כמו מחלקה)
            if (string.IsNullOrEmpty(personId) || entityId <= 0 || string.IsNullOrEmpty(requiredRole))
                return false;

            // בדיקה אם המשתמש הוא מנהל מערכת
            if (_roleService.IsAdmin(personId))
                return true;

            // יש להוסיף בדיקות אחרות לפי צורכי המערכת
            return false;
        }

        public bool CanAccessForm(string personId, int formId)
        {
            if (string.IsNullOrEmpty(personId) || formId <= 0)
                return false;

            // בדיקה אם המשתמש הוא מנהל מערכת או חבר ועדה
            if (_roleService.IsAdmin(personId) || _roleService.IsCommitteeMember(personId))
                return true;

            // יש להוסיף בדיקות אחרות לפי צורכי המערכת
            return false;
        }

        public bool CanManageUsers(string personId)
        {
            // רק מנהל מערכת יכול לנהל משתמשים
            return !string.IsNullOrEmpty(personId) && _roleService.IsAdmin(personId);
        }
    }
}