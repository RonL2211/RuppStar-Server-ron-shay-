using FinalProject.DAL.Models;
using FinalProject.DAL.Repositories;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FinalProject.BL.Services
{
    public class FormInstanceService
    {
        private readonly FormInstanceRepository _instanceRepository;
        private readonly FormRepository _formRepository;
        private readonly PersonRepository _personRepository;

        public FormInstanceService(IConfiguration configuration)
        {
            _instanceRepository = new FormInstanceRepository(configuration);
            _formRepository = new FormRepository(configuration);
            _personRepository = new PersonRepository(configuration);
        }

        public List<FormInstance> GetUserInstances(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentException("User ID cannot be empty");

            return _instanceRepository.GetInstancesByUserId(userId);
        }

        public FormInstance GetInstanceById(int instanceId)
        {
            if (instanceId <= 0)
                throw new ArgumentException("Instance ID must be greater than zero");

            return _instanceRepository.GetInstanceById(instanceId);
        }

        public int CreateInstance(string userId, int formId)
        {
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentException("User ID cannot be empty");

            if (formId <= 0)
                throw new ArgumentException("Form ID must be greater than zero");

            // בדיקה שהטופס קיים ופעיל
            var form = _formRepository.GetFormById(formId);
            if (form == null)
                throw new ArgumentException($"Form with ID {formId} does not exist");

            if (!form.IsActive || !form.IsPublished)
                throw new InvalidOperationException("Cannot create an instance for an inactive or unpublished form");

            // בדיקה שהמשתמש קיים
            var person = _personRepository.GetPersonById(userId);
            if (person == null)
                throw new ArgumentException($"User with ID {userId} does not exist");

            // בדיקה שלא קיים כבר מופע של טופס זה למשתמש זה
            var existingInstances = _instanceRepository.GetInstancesByUserId(userId)
                .Where(i => i.FormId == formId && i.CurrentStage != "Rejected" && i.CurrentStage != "Closed")
                .ToList();

            if (existingInstances.Any())
                throw new InvalidOperationException("User already has an active instance of this form");

            // יצירת מופע הטופס
            var instance = new FormInstance
            {
                FormId = formId,
                UserID = userId,
                CreatedDate = DateTime.Now,
                CurrentStage = "Draft",
                TotalScore = 0,
                LastModifiedDate = DateTime.Now
            };

            return _instanceRepository.CreateInstance(instance);
        }

        public int SubmitInstance(int instanceId, string comments = null)
        {
            if (instanceId <= 0)
                throw new ArgumentException("Instance ID must be greater than zero");

            // בדיקה שהמופע קיים
            var instance = _instanceRepository.GetInstanceById(instanceId);
            if (instance == null)
                throw new ArgumentException($"Instance with ID {instanceId} does not exist");

            // בדיקה שהמופע במצב טיוטה
            if (instance.CurrentStage != "Draft")
                throw new InvalidOperationException("Cannot submit an instance that is not in Draft status");

            // עדכון סטטוס המופע ל-'הוגש'
            instance.CurrentStage = "Submitted";
            instance.SubmissionDate = DateTime.Now;
            instance.Comments = comments;

            return _instanceRepository.UpdateInstanceStatus(instanceId, instance.CurrentStage, comments);
        }

        public int ApproveInstance(int instanceId, decimal totalScore, string comments = null)
        {
            if (instanceId <= 0)
                throw new ArgumentException("Instance ID must be greater than zero");

            // בדיקה שהמופע קיים
            var instance = _instanceRepository.GetInstanceById(instanceId);
            if (instance == null)
                throw new ArgumentException($"Instance with ID {instanceId} does not exist");

            // בדיקה שהמופע במצב 'הוגש'
            if (instance.CurrentStage != "Submitted" && instance.CurrentStage != "UnderReview")
                throw new InvalidOperationException("Cannot approve an instance that is not in Submitted or UnderReview status");

            // עדכון סטטוס המופע ל-'מאושר'
            instance.CurrentStage = "Approved";
            instance.TotalScore = totalScore;
            instance.LastModifiedDate = DateTime.Now;
            instance.Comments = comments;

            _instanceRepository.UpdateInstanceStatus(instanceId, instance.CurrentStage, comments);
            return _instanceRepository.SubmitInstance(instanceId, totalScore);
        }

        public int RejectInstance(int instanceId, string comments)
        {
            if (instanceId <= 0)
                throw new ArgumentException("Instance ID must be greater than zero");

            if (string.IsNullOrEmpty(comments))
                throw new ArgumentException("Comments are required for rejection");

            // בדיקה שהמופע קיים
            var instance = _instanceRepository.GetInstanceById(instanceId);
            if (instance == null)
                throw new ArgumentException($"Instance with ID {instanceId} does not exist");

            // בדיקה שהמופע במצב 'הוגש'
            if (instance.CurrentStage != "Submitted" && instance.CurrentStage != "UnderReview")
                throw new InvalidOperationException("Cannot reject an instance that is not in Submitted or UnderReview status");

            // עדכון סטטוס המופע ל-'נדחה'
            instance.CurrentStage = "Rejected";
            instance.LastModifiedDate = DateTime.Now;
            instance.Comments = comments;

            return _instanceRepository.UpdateInstanceStatus(instanceId, instance.CurrentStage, comments);
        }

        public int ReturnForRevision(int instanceId, string comments)
        {
            if (instanceId <= 0)
                throw new ArgumentException("Instance ID must be greater than zero");

            if (string.IsNullOrEmpty(comments))
                throw new ArgumentException("Comments are required for return");

            // בדיקה שהמופע קיים
            var instance = _instanceRepository.GetInstanceById(instanceId);
            if (instance == null)
                throw new ArgumentException($"Instance with ID {instanceId} does not exist");

            // בדיקה שהמופע במצב 'הוגש'
            if (instance.CurrentStage != "Submitted" && instance.CurrentStage != "UnderReview")
                throw new InvalidOperationException("Cannot return an instance that is not in Submitted or UnderReview status");

            // עדכון סטטוס המופע ל-'חזר לתיקונים'
            instance.CurrentStage = "ReturnedForRevision";
            instance.LastModifiedDate = DateTime.Now;
            instance.Comments = comments;

            return _instanceRepository.UpdateInstanceStatus(instanceId, instance.CurrentStage, comments);
        }

        public int MarkAsUnderReview(int instanceId, string comments = null)
        {
            if (instanceId <= 0)
                throw new ArgumentException("Instance ID must be greater than zero");

            // בדיקה שהמופע קיים
            var instance = _instanceRepository.GetInstanceById(instanceId);
            if (instance == null)
                throw new ArgumentException($"Instance with ID {instanceId} does not exist");

            // בדיקה שהמופע במצב 'הוגש'
            if (instance.CurrentStage != "Submitted")
                throw new InvalidOperationException("Cannot mark as under review an instance that is not in Submitted status");

            // עדכון סטטוס המופע ל-'בבדיקה'
            instance.CurrentStage = "UnderReview";
            instance.LastModifiedDate = DateTime.Now;
            instance.Comments = comments;

            return _instanceRepository.UpdateInstanceStatus(instanceId, instance.CurrentStage, comments);
        }

        public int AppealInstance(int instanceId, string appealReason)
        {
            if (instanceId <= 0)
                throw new ArgumentException("Instance ID must be greater than zero");

            if (string.IsNullOrEmpty(appealReason))
                throw new ArgumentException("Appeal reason is required");

            // בדיקה שהמופע קיים
            var instance = _instanceRepository.GetInstanceById(instanceId);
            if (instance == null)
                throw new ArgumentException($"Instance with ID {instanceId} does not exist");

            // בדיקה שהמופע במצב 'נדחה' או 'מאושר'
            if (instance.CurrentStage != "Rejected" && instance.CurrentStage != "Approved")
                throw new InvalidOperationException("Cannot appeal an instance that is not in Rejected or Approved status");

            // עדכון סטטוס המופע ל-'בערעור'
            instance.CurrentStage = "UnderAppeal";
            instance.LastModifiedDate = DateTime.Now;
            instance.Comments = appealReason;

            return _instanceRepository.UpdateInstanceStatus(instanceId, instance.CurrentStage, appealReason);
        }

        public List<FormInstance> GetInstancesByStage(string stage)
        {
            if (string.IsNullOrEmpty(stage))
                throw new ArgumentException("Stage cannot be empty");

            return _instanceRepository.GetInstancesByStage(stage);
        }

        public List<FormInstance> GetInstancesByFormId(int formId)
        {
            if (formId <= 0)
                throw new ArgumentException("Form ID must be greater than zero");

            return _instanceRepository.GetInstancesByFormId(formId);
        }

        public Dictionary<string, int> GetInstancesStatisticsByForm(int formId)
        {
            if (formId <= 0)
                throw new ArgumentException("Form ID must be greater than zero");

            var instances = _instanceRepository.GetInstancesByFormId(formId);
            var stats = new Dictionary<string, int>();

            // חישוב סטטיסטיקות לפי סטטוס
            var stages = instances.Select(i => i.CurrentStage).Distinct().ToList();
            foreach (var stage in stages)
            {
                stats.Add(stage, instances.Count(i => i.CurrentStage == stage));
            }

            // הוספת סך הכל
            stats.Add("Total", instances.Count);

            return stats;
        }
    }
}