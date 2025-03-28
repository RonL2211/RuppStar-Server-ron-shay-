using FinalProject.DAL.Models;
using FinalProject.DAL.Repositories;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;

namespace FinalProject.BL.Services
{
    public class RoleService
    {
        private readonly PersonRepository _personRepository;

        public RoleService(IConfiguration configuration)
        {
            _personRepository = new PersonRepository(configuration);
        }

        public List<Role> GetPersonRoles(string personId)
        {
            if (string.IsNullOrEmpty(personId))
                throw new ArgumentException("Person ID cannot be empty");

            return _personRepository.GetPersonRoles(personId);
        }

        public bool IsInRole(string personId, string roleName)
        {
            var roles = GetPersonRoles(personId);
            return roles.Exists(r => r.RoleName.Equals(roleName, StringComparison.OrdinalIgnoreCase));
        }

        public bool IsFacultyHead(string personId)
        {
            return IsInRole(personId, "FacultyHead");
        }

        public bool IsCommitteeMember(string personId)
        {
            return IsInRole(personId, "CommitteeMember");
        }

        public bool IsInstructor(string personId)
        {
            return IsInRole(personId, "Instructor");
        }

        public bool IsAdmin(string personId)
        {
            return IsInRole(personId, "Admin");
        }

        public int AssignRole(string personId, int roleId)
        {
            if (string.IsNullOrEmpty(personId))
                throw new ArgumentException("Person ID cannot be empty");

            if (roleId <= 0)
                throw new ArgumentException("Role ID must be greater than zero");

            return _personRepository.AssignRoleToPerson(personId, roleId);
        }

        public int RemoveRole(string personId, int roleId)
        {
            if (string.IsNullOrEmpty(personId))
                throw new ArgumentException("Person ID cannot be empty");

            if (roleId <= 0)
                throw new ArgumentException("Role ID must be greater than zero");

            return _personRepository.RemoveRoleFromPerson(personId, roleId);
        }
    }
}

