using FinalProject.DAL.Models;
using FinalProject.DAL.Repositories;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace FinalProject.BL.Services
{
    public class PersonService
    {
        private readonly PersonRepository _personRepository;

        public PersonService(IConfiguration configuration)
        {
            _personRepository = new PersonRepository(configuration);
        }

        public List<Person> GetAllPersons()
        {
            return _personRepository.GetAllPersons();
        }

        public Person GetPersonById(string personId)
        {
            if (string.IsNullOrEmpty(personId))
                throw new ArgumentException("Person ID cannot be empty");

            return _personRepository.GetPersonById(personId);
        }

        public Person GetPersonByUsername(string username)
        {
            if (string.IsNullOrEmpty(username))
                throw new ArgumentException("Username cannot be empty");

            return _personRepository.GetPersonByUsername(username);
        }

        public bool AuthenticateUser(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                return false;

            var person = _personRepository.GetPersonByUsername(username);
            if (person == null)
                return false;

            // בדיקה פשוטה של סיסמה (במצב אמיתי כדאי להשתמש בהצפנה חזקה יותר)
            return person.Password == HashPassword(password);
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

        public int AddPerson(Person person)
        {
            ValidatePerson(person);

            // הצפנת הסיסמה לפני שמירה
            person.Password = HashPassword(person.Password);

            return _personRepository.AddPerson(person);
        }

        public int UpdatePerson(Person person)
        {
            ValidatePerson(person);

            // לא לשנות את הסיסמה אם היא ריקה (אולי המשתמש לא רוצה לשנות את הסיסמה)
            if (string.IsNullOrEmpty(person.Password))
            {
                var existingPerson = _personRepository.GetPersonById(person.PersonId);
                person.Password = existingPerson.Password;
            }
            else
            {
                person.Password = HashPassword(person.Password);
            }

            return _personRepository.UpdatePerson(person);
        }

        private void ValidatePerson(Person person)
        {
            if (person == null)
                throw new ArgumentNullException(nameof(person), "Person cannot be null");

            if (string.IsNullOrEmpty(person.PersonId))
                throw new ArgumentException("Person ID is required");

            if (string.IsNullOrEmpty(person.Username))
                throw new ArgumentException("Username is required");

            if (string.IsNullOrEmpty(person.FirstName))
                throw new ArgumentException("First name is required");

            if (string.IsNullOrEmpty(person.LastName))
                throw new ArgumentException("Last name is required");

            if (string.IsNullOrEmpty(person.Email))
                throw new ArgumentException("Email is required");

            // וולידציה נוספת לפי הצורך
        }

        public List<Role> GetPersonRoles(string personId)
        {
            if (string.IsNullOrEmpty(personId))
                throw new ArgumentException("Person ID cannot be empty");

            return _personRepository.GetPersonRoles(personId);
        }

        public bool IsPersonInRole(string personId, string roleName)
        {
            if (string.IsNullOrEmpty(personId) || string.IsNullOrEmpty(roleName))
                return false;

            var roles = _personRepository.GetPersonRoles(personId);
            return roles.Any(r => r.RoleName.Equals(roleName, StringComparison.OrdinalIgnoreCase));
        }

        public int AssignRoleToPerson(string personId, int roleId)
        {
            if (string.IsNullOrEmpty(personId))
                throw new ArgumentException("Person ID cannot be empty");

            return _personRepository.AssignRoleToPerson(personId, roleId);
        }

        public int RemoveRoleFromPerson(string personId, int roleId)
        {
            if (string.IsNullOrEmpty(personId))
                throw new ArgumentException("Person ID cannot be empty");

            return _personRepository.RemoveRoleFromPerson(personId, roleId);
        }
    }
}