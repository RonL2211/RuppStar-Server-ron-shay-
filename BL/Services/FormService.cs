using FinalProject.DAL.Models;
using FinalProject.DAL.Repositories;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FinalProject.BL.Services
{
    public class FormService
    {
        private readonly FormRepository _formRepository;
        private readonly FormSectionRepository _sectionRepository;
        private readonly SectionFieldRepository _fieldRepository;

        public FormService(IConfiguration configuration)
        {
            _formRepository = new FormRepository(configuration);
            _sectionRepository = new FormSectionRepository(configuration);
            _fieldRepository = new SectionFieldRepository(configuration);
        }

        public List<Form> GetAllForms()
        {
            return _formRepository.GetAllForms();
        }

        public Form GetFormById(int formId)
        {
            if (formId <= 0)
                throw new ArgumentException("Form ID must be greater than zero");

            return _formRepository.GetFormById(formId);
        }

        public List<Form> GetFormsByAcademicYear(string academicYear)
        {
            if (string.IsNullOrEmpty(academicYear))
                throw new ArgumentException("Academic year cannot be empty");

            return _formRepository.GetFormsByAcademicYear(academicYear);
        }

        public List<Form> GetActiveForms()
        {
            return _formRepository.GetAllForms().Where(f => f.IsActive).ToList();
        }

        public List<Form> GetPublishedForms()
        {
            return _formRepository.GetAllForms().Where(f => f.IsPublished && f.IsActive).ToList();
        }

        public int CreateForm(Form form)
        {
            ValidateForm(form);

            // הגדרות ברירת מחדל
            form.CreationDate = DateTime.Now;
            form.LastModifiedDate = DateTime.Now;
            form.IsActive = true;
            form.IsPublished = false;

            return _formRepository.AddForm(form);
        }

        public int UpdateForm(Form form)
        {
            ValidateForm(form);

            // עדכון תאריך עדכון
            form.LastModifiedDate = DateTime.Now;

            return _formRepository.UpdateForm(form);
        }

        public int PublishForm(int formId, string modifiedBy)
        {
            if (formId <= 0)
                throw new ArgumentException("Form ID must be greater than zero");

            if (string.IsNullOrEmpty(modifiedBy))
                throw new ArgumentException("Modified by cannot be empty");

            // בדיקה שהטופס קיים ופעיל
            var form = _formRepository.GetFormById(formId);
            if (form == null)
                throw new ArgumentException($"Form with ID {formId} does not exist");

            if (!form.IsActive)
                throw new InvalidOperationException("Cannot publish an inactive form");

            return _formRepository.PublishForm(formId, modifiedBy);
        }

        private void ValidateForm(Form form)
        {
            if (form == null)
                throw new ArgumentNullException(nameof(form), "Form cannot be null");

            if (string.IsNullOrEmpty(form.FormName))
                throw new ArgumentException("Form name is required");

            if (string.IsNullOrEmpty(form.AcademicYear))
                throw new ArgumentException("Academic year is required");

            if (string.IsNullOrEmpty(form.CreatedBy))
                throw new ArgumentException("Created by is required");

            // וולידציה נוספת לפי הצורך
        }

        // שירותים לניהול מבנה הטופס (סעיפים ושדות)

        public List<FormSection> GetFormStructure(int formId)
        {
            if (formId <= 0)
                throw new ArgumentException("Form ID must be greater than zero");

            var allSections = _sectionRepository.GetSectionsByFormId(formId);

            // ארגון הסעיפים בצורה היררכית
            var rootSections = allSections.Where(s => s.ParentSectionID == null).OrderBy(s => s.OrderIndex).ToList();

            return rootSections;
        }

        public Dictionary<int, List<FormSection>> GetFormSectionHierarchy(int formId)
        {
            if (formId <= 0)
                throw new ArgumentException("Form ID must be greater than zero");

            var allSections = _sectionRepository.GetSectionsByFormId(formId);
            var sectionHierarchy = new Dictionary<int, List<FormSection>>();

            // מאתר את כל הסעיפים ברמה הראשונה
            var rootSections = allSections.Where(s => s.ParentSectionID == null).OrderBy(s => s.OrderIndex).ToList();
            sectionHierarchy.Add(0, rootSections);

            // מסדר את יתר הסעיפים לפי הסעיף ההורה
            foreach (var section in allSections.Where(s => s.ParentSectionID != null))
            {
                var parentId = section.ParentSectionID.Value;
                if (!sectionHierarchy.ContainsKey(parentId))
                {
                    sectionHierarchy.Add(parentId, new List<FormSection>());
                }

                sectionHierarchy[parentId].Add(section);
            }

            // ממיין את הסעיפים בכל רמה לפי סדר
            foreach (var key in sectionHierarchy.Keys.ToList())
            {
                sectionHierarchy[key] = sectionHierarchy[key].OrderBy(s => s.OrderIndex).ToList();
            }

            return sectionHierarchy;
        }

        public List<SectionField> GetSectionFields(int sectionId)
        {
            if (sectionId <= 0)
                throw new ArgumentException("Section ID must be greater than zero");

            return _fieldRepository.GetFieldsBySectionId(sectionId).OrderBy(f => f.OrderIndex).ToList();
        }

        public int AddSection(FormSection section)
        {
            ValidateSection(section);

            // עדכון רמת הסעיף בהתאם להורה
            if (section.ParentSectionID.HasValue)
            {
                var parentSection = _sectionRepository.GetSectionById(section.ParentSectionID.Value);
                if (parentSection != null)
                {
                    section.Level = (byte)(parentSection.Level + 1);
                }
            }
            else
            {
                section.Level = 1; // סעיף ברמה הראשונה
            }

            return _sectionRepository.AddSection(section);
        }

        public int UpdateSection(FormSection section)
        {
            ValidateSection(section);
            return _sectionRepository.UpdateSection(section);
        }

        public int DeleteSection(int sectionId)
        {
            if (sectionId <= 0)
                throw new ArgumentException("Section ID must be greater than zero");

            // בדיקה האם יש סעיפי משנה - אם כן, לא ניתן למחוק
            var childSections = _sectionRepository.GetChildSections(sectionId);
            if (childSections.Any())
            {
                throw new InvalidOperationException("Cannot delete a section that has child sections");
            }

            // בדיקה האם יש שדות - אם כן, צריך למחוק גם אותם
            var fields = _fieldRepository.GetFieldsBySectionId(sectionId);
            foreach (var field in fields)
            {
                _fieldRepository.DeleteField(field.FieldID);
            }

            return _sectionRepository.DeleteSection(sectionId);
        }

        private void ValidateSection(FormSection section)
        {
            if (section == null)
                throw new ArgumentNullException(nameof(section), "Section cannot be null");

            if (section.FormId <= 0)
                throw new ArgumentException("Form ID must be greater than zero");

            if (string.IsNullOrEmpty(section.Title))
                throw new ArgumentException("Section title is required");

            // בדיקה שההורה קיים אם מוגדר
            if (section.ParentSectionID.HasValue)
            {
                var parentSection = _sectionRepository.GetSectionById(section.ParentSectionID.Value);
                if (parentSection == null)
                {
                    throw new ArgumentException($"Parent section with ID {section.ParentSectionID} does not exist");
                }

                // בדיקה שההורה שייך לאותו טופס
                if (parentSection.FormId != section.FormId)
                {
                    throw new ArgumentException("Parent section must be in the same form");
                }
            }
        }

        public int AddField(SectionField field)
        {
            ValidateField(field);
            return _fieldRepository.AddField(field);
        }

        public int UpdateField(SectionField field)
        {
            ValidateField(field);
            return _fieldRepository.UpdateField(field);
        }

        public int DeleteField(int fieldId)
        {
            if (fieldId <= 0)
                throw new ArgumentException("Field ID must be greater than zero");

            // בדיקה האם יש אפשרויות לשדה - אם כן, צריך למחוק גם אותן
            var options = _fieldRepository.GetFieldOptions(fieldId);
            foreach (var option in options)
            {
                _fieldRepository.DeleteFieldOption(option.OptionID);
            }

            return _fieldRepository.DeleteField(fieldId);
        }

        private void ValidateField(SectionField field)
        {
            if (field == null)
                throw new ArgumentNullException(nameof(field), "Field cannot be null");

            if (field.SectionID == null || field.SectionID <= 0)
                throw new ArgumentException("Section ID must be greater than zero");

            if (string.IsNullOrEmpty(field.FieldName))
                throw new ArgumentException("Field name is required");

            if (string.IsNullOrEmpty(field.FieldLabel))
                throw new ArgumentException("Field label is required");

            if (string.IsNullOrEmpty(field.FieldType))
                throw new ArgumentException("Field type is required");

            // וולידציה נוספת בהתאם לסוג השדה
            switch (field.FieldType.ToLower())
            {
                case "number":
                    if (field.MinValue.HasValue && field.MaxValue.HasValue && field.MinValue > field.MaxValue)
                    {
                        throw new ArgumentException("Min value cannot be greater than max value");
                    }
                    break;

                case "text":
                    if (field.MaxLength <= 0)
                    {
                        field.MaxLength = 255; // ערך ברירת מחדל
                    }
                    break;
            }
        }

        public List<FieldOption> GetFieldOptions(int fieldId)
        {
            if (fieldId <= 0)
                throw new ArgumentException("Field ID must be greater than zero");

            return _fieldRepository.GetFieldOptions(fieldId).OrderBy(o => o.OrderIndex).ToList();
        }

        public int AddFieldOption(FieldOption option)
        {
            ValidateFieldOption(option);
            return _fieldRepository.AddFieldOption(option);
        }

        public int UpdateFieldOption(FieldOption option)
        {
            ValidateFieldOption(option);
            return _fieldRepository.UpdateFieldOption(option);
        }

        public int DeleteFieldOption(int optionId)
        {
            if (optionId <= 0)
                throw new ArgumentException("Option ID must be greater than zero");

            return _fieldRepository.DeleteFieldOption(optionId);
        }
        public SectionField GetFieldById(int fieldId)
        {
            if (fieldId <= 0)
                throw new ArgumentException("Field ID must be greater than zero");

            return _fieldRepository.GetFieldById(fieldId);
        }

        public FormSection GetSectionById(int sectionId)
        {
            if (sectionId <= 0)
                throw new ArgumentException("Section ID must be greater than zero");

            return _sectionRepository.GetSectionById(sectionId);
        }

        public FieldOption GetFieldOptionById(int optionId)
        {
            if (optionId <= 0)
                throw new ArgumentException("Option ID must be greater than zero");

            return _fieldRepository.GetFieldOptionById(optionId);
        }
        private void ValidateFieldOption(FieldOption option)
        {
            if (option == null)
                throw new ArgumentNullException(nameof(option), "Option cannot be null");

            if (option.FieldID <= 0)
                throw new ArgumentException("Field ID must be greater than zero");

            if (string.IsNullOrEmpty(option.OptionValue))
                throw new ArgumentException("Option value is required");

            if (string.IsNullOrEmpty(option.OptionLabel))
                throw new ArgumentException("Option label is required");
        }
    }
}