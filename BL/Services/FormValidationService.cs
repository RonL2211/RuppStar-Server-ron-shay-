using FinalProject.DAL.Models;
using FinalProject.DAL.Repositories;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FinalProject.BL.Services
{
    public class FormValidationService
    {
        private readonly FormRepository _formRepository;
        private readonly FormSectionRepository _sectionRepository;
        private readonly SectionFieldRepository _fieldRepository;
        private readonly IConfiguration _configuration;


        public FormValidationService(IConfiguration configuration)
        {
            _configuration = configuration; 
            _formRepository = new FormRepository(configuration);
            _sectionRepository = new FormSectionRepository(configuration);
            _fieldRepository = new SectionFieldRepository(configuration);
        }

        public bool ValidateForm(int formId)
        {
            if (formId <= 0)
                throw new ArgumentException("Form ID must be greater than zero");

            // בדיקה שהטופס קיים
            var form = _formRepository.GetFormById(formId);
            if (form == null)
                return false;

            // בדיקה שהטופס מכיל לפחות סעיף אחד
            var sections = _sectionRepository.GetSectionsByFormId(formId);
            if (!sections.Any())
                return false;

            // בדיקה שכל סעיף מכיל לפחות שדה אחד
            foreach (var section in sections)
            {
                var fields = _fieldRepository.GetFieldsBySectionId(section.SectionID);
                if (!fields.Any())
                    return false;
            }

            return true;
        }

        public Dictionary<string, string> ValidateFormStructure(int formId)
        {
            var validationErrors = new Dictionary<string, string>();

            if (formId <= 0)
            {
                validationErrors.Add("FormId", "Form ID must be greater than zero");
                return validationErrors;
            }

            // בדיקה שהטופס קיים
            var form = _formRepository.GetFormById(formId);
            if (form == null)
            {
                validationErrors.Add("Form", $"Form with ID {formId} does not exist");
                return validationErrors;
            }

            // בדיקה שהטופס מכיל לפחות סעיף אחד
            var sections = _sectionRepository.GetSectionsByFormId(formId);
            if (!sections.Any())
            {
                validationErrors.Add("Sections", "Form must contain at least one section");
                return validationErrors;
            }

            // בדיקת מבנה הסעיפים
            foreach (var section in sections)
            {
                // בדיקה שהסעיף שייך לטופס הנכון
                if (section.FormId != formId)
                {
                    validationErrors.Add($"Section_{section.SectionID}", $"Section belongs to a different form (Form ID: {section.FormId})");
                }

                // בדיקה שההורה של הסעיף תקין (אם יש)
                if (section.ParentSectionID.HasValue)
                {
                    var parentExists = sections.Any(s => s.SectionID == section.ParentSectionID.Value);
                    if (!parentExists)
                    {
                        validationErrors.Add($"Section_{section.SectionID}_Parent", $"Parent section with ID {section.ParentSectionID} does not exist");
                    }
                }

                // בדיקה שהרמה של הסעיף תואמת את המבנה ההיררכי
                if (section.ParentSectionID.HasValue)
                {
                    var parentSection = sections.First(s => s.SectionID == section.ParentSectionID.Value);
                    if (section.Level != parentSection.Level + 1)
                    {
                        validationErrors.Add($"Section_{section.SectionID}_Level", $"Section level ({section.Level}) does not match parent level + 1 ({parentSection.Level + 1})");
                    }
                }
                else if (section.Level != 1)
                {
                    validationErrors.Add($"Section_{section.SectionID}_Level", $"Root section level must be 1 (Current: {section.Level})");
                }

                // בדיקה שהסעיף מכיל לפחות שדה אחד
                var fields = _fieldRepository.GetFieldsBySectionId(section.SectionID);
                if (!fields.Any())
                {
                    validationErrors.Add($"Section_{section.SectionID}_Fields", $"Section must contain at least one field");
                }

                // בדיקת תקינות השדות
                foreach (var field in fields)
                {
                    if (field.SectionID != section.SectionID)
                    {
                        validationErrors.Add($"Field_{field.FieldID}_Section", $"Field belongs to a different section (Section ID: {field.SectionID})");
                    }

                    // בדיקת תקינות סוג השדה
                    if (string.IsNullOrEmpty(field.FieldType))
                    {
                        validationErrors.Add($"Field_{field.FieldID}_Type", "Field type is required");
                    }
                    else
                    {
                        // בדיקות תקינות נוספות לפי סוג השדה
                        switch (field.FieldType.ToLower())
                        {
                            case "number":
                                if (field.MinValue.HasValue && field.MaxValue.HasValue && field.MinValue > field.MaxValue)
                                {
                                    validationErrors.Add($"Field_{field.FieldID}_Range", "Min value cannot be greater than max value");
                                }
                                break;

                            case "select":
                            case "radio":
                            case "checkbox":
                                var options = _fieldRepository.GetFieldOptions(field.FieldID);
                                if (!options.Any())
                                {
                                    validationErrors.Add($"Field_{field.FieldID}_Options", "Field of type select/radio/checkbox must have at least one option");
                                }
                                break;
                        }
                    }
                }
            }

            return validationErrors;
        }

        public Dictionary<string, List<string>> ValidateFormInstance(int instanceId)
        {
            var validationErrors = new Dictionary<string, List<string>>();

            // כאן יש לממש בדיקת תקינות של מופע הטופס שמולא על ידי המשתמש
            // למשל: בדיקה שכל השדות החובה מולאו, שהערכים בטווח המותר, וכו'

            // דוגמה פשוטה לרעיון:

            // קבלת מופע הטופס
            var instanceRepository = new FormInstanceRepository(_configuration);
            var instance = instanceRepository.GetInstanceById(instanceId);
            if (instance == null)
            {
                validationErrors.Add("Instance", new List<string> { $"Instance with ID {instanceId} does not exist" });
                return validationErrors;
            }

            // קבלת הטופס
            var form = _formRepository.GetFormById(instance.FormId);
            if (form == null)
            {
                validationErrors.Add("Form", new List<string> { $"Form with ID {instance.FormId} does not exist" });
                return validationErrors;
            }

            // קבלת כל הסעיפים בטופס
            var sections = _sectionRepository.GetSectionsByFormId(instance.FormId);

            // לכל סעיף, בדיקת תקינות השדות
            foreach (var section in sections)
            {
                // האם הסעיף חובה
                if (section.IsRequired)
                {
                    // בדיקת תקינות השדות בסעיף
                    var fields = _fieldRepository.GetFieldsBySectionId(section.SectionID);
                    foreach (var field in fields)
                    {
                        if (field.IsRequired)
                        {
                            // כאן צריך לבדוק אם השדה מולא על ידי המשתמש
                            // בהנחה שיש טבלה שמכילה את הערכים שהמשתמש הזין
                            var fieldValue = GetFieldValue(instanceId, field.FieldID);
                            if (string.IsNullOrEmpty(fieldValue))
                            {
                                if (!validationErrors.ContainsKey($"Section_{section.SectionID}"))
                                {
                                    validationErrors.Add($"Section_{section.SectionID}", new List<string>());
                                }

                                validationErrors[$"Section_{section.SectionID}"].Add($"Field '{field.FieldLabel}' is required");
                            }
                        }
                    }
                }
            }

            return validationErrors;
        }

        private string GetFieldValue(int instanceId, int fieldId)
        {
            // פונקציה זו אמורה לקבל את הערך שהמשתמש הזין לשדה מסוים
            // יש ליישם את הגישה למסד הנתונים לקבלת הערך

            // דוגמה לערך דמה
            return "Dummy Value";
        }
    }
}