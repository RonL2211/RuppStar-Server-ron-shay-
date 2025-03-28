using FinalProject.DAL.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace FinalProject.BL.Services
{
    public class AuditTrailService
    {
        private readonly IConfiguration _configuration;
        private readonly string _auditTrailPath;

        public AuditTrailService(IConfiguration configuration)
        {
            _configuration = configuration;
            _auditTrailPath = _configuration["AuditTrail:FilePath"] ?? "AuditTrail.json";
        }

        public async Task LogActionAsync(string userId, string action, string entityType, int entityId, string details)
        {
            var auditEntry = new AuditTrailEntry
            {
                Timestamp = DateTime.Now,
                UserId = userId,
                Action = action,
                EntityType = entityType,
                EntityId = entityId,
                Details = details
            };

            try
            {
                await AppendToAuditTrailAsync(auditEntry);
            }
            catch (Exception ex)
            {
                // לוג שגיאות
                Console.WriteLine($"Error logging audit trail: {ex.Message}");
                // במערכת אמיתית יש לנהל לוג שגיאות מסודר
            }
        }

        private async Task AppendToAuditTrailAsync(AuditTrailEntry entry)
        {
            // אם הקובץ לא קיים, ניצור אותו
            if (!File.Exists(_auditTrailPath))
            {
                var initialEntries = new List<AuditTrailEntry> { entry };
                var initialJson = JsonSerializer.Serialize(initialEntries, new JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync(_auditTrailPath, initialJson);
                return;
            }

            // קריאת הקובץ הקיים
            var existingJson = await File.ReadAllTextAsync(_auditTrailPath);
            var auditTrail = JsonSerializer.Deserialize<List<AuditTrailEntry>>(existingJson) ?? new List<AuditTrailEntry>();

            // הוספת הרשומה החדשה
            auditTrail.Add(entry);

            // שמירת הקובץ המעודכן
            var updatedJson = JsonSerializer.Serialize(auditTrail, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(_auditTrailPath, updatedJson);
        }

        public async Task<List<AuditTrailEntry>> GetAuditTrailAsync(
            DateTime? fromDate = null,
            DateTime? toDate = null,
            string userId = null,
            string action = null,
            string entityType = null)
        {
            if (!File.Exists(_auditTrailPath))
                return new List<AuditTrailEntry>();

            var existingJson = await File.ReadAllTextAsync(_auditTrailPath);
            var auditTrail = JsonSerializer.Deserialize<List<AuditTrailEntry>>(existingJson) ?? new List<AuditTrailEntry>();

            // סינון לפי הפרמטרים
            var filteredTrail = auditTrail.FindAll(entry =>
                (!fromDate.HasValue || entry.Timestamp >= fromDate.Value) &&
                (!toDate.HasValue || entry.Timestamp <= toDate.Value) &&
                (string.IsNullOrEmpty(userId) || entry.UserId == userId) &&
                (string.IsNullOrEmpty(action) || entry.Action == action) &&
                (string.IsNullOrEmpty(entityType) || entry.EntityType == entityType)
            );

            return filteredTrail;
        }

        public async Task<List<AuditTrailEntry>> GetUserActionsAsync(string userId)
        {
            return await GetAuditTrailAsync(userId: userId);
        }

        public async Task<List<AuditTrailEntry>> GetEntityHistoryAsync(string entityType, int entityId)
        {
            if (!File.Exists(_auditTrailPath))
                return new List<AuditTrailEntry>();

            var existingJson = await File.ReadAllTextAsync(_auditTrailPath);
            var auditTrail = JsonSerializer.Deserialize<List<AuditTrailEntry>>(existingJson) ?? new List<AuditTrailEntry>();

            // סינון לפי סוג הישות ומזהה הישות
            var filteredTrail = auditTrail.FindAll(entry =>
                entry.EntityType == entityType && entry.EntityId == entityId
            );

            return filteredTrail;
        }
    }

    public class AuditTrailEntry
    {
        public DateTime Timestamp { get; set; }
        public string UserId { get; set; }
        public string Action { get; set; }
        public string EntityType { get; set; }
        public int EntityId { get; set; }
        public string Details { get; set; }
    }
}