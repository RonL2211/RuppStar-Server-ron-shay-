using FinalProject.DAL.Models;
using FinalProject.DAL.Repositories;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace FinalProject.BL.Services
{
    public class NotificationService
    {
        private readonly PersonRepository _personRepository;
        private readonly FormRepository _formRepository;
        private readonly FormInstanceRepository _instanceRepository;
        private readonly IConfiguration _configuration;

        public NotificationService(IConfiguration configuration)
        {
            _personRepository = new PersonRepository(configuration);
            _formRepository = new FormRepository(configuration);
            _instanceRepository = new FormInstanceRepository(configuration);
            _configuration = configuration;
        }

        public async Task SendFormSubmissionNotificationAsync(int instanceId)
        {
            // קבלת מופע הטופס
            var instance = _instanceRepository.GetInstanceById(instanceId);
            if (instance == null)
                throw new ArgumentException($"Instance with ID {instanceId} does not exist");

            // קבלת הטופס
            var form = _formRepository.GetFormById(instance.FormId);
            if (form == null)
                throw new ArgumentException($"Form with ID {instance.FormId} does not exist");

            // קבלת המשתמש
            var person = _personRepository.GetPersonById(instance.UserID);
            if (person == null)
                throw new ArgumentException($"Person with ID {instance.UserID} does not exist");

            // קבלת ראש הפקולטה
            // בהנחה שיש שדה במופע הטופס שמציין למי לשלוח את ההתראה
            var facultyHeadId = GetResponsiblePersonForInstance(instanceId);
            var facultyHead = _personRepository.GetPersonById(facultyHeadId);
            if (facultyHead == null)
                throw new ArgumentException($"Faculty head with ID {facultyHeadId} does not exist");

            // שליחת התראה למשתמש
            await SendEmailAsync(
                person.Email,
                $"Submission Confirmation - {form.FormName}",
                $"Dear {person.FirstName} {person.LastName},\n\n" +
                $"We confirm the receipt of your submission for {form.FormName}. " +
                $"Your submission is now under review.\n\n" +
                $"Submission Date: {instance.SubmissionDate}\n" +
                $"Status: {instance.CurrentStage}\n\n" +
                $"You will be notified when there is any update on your submission.\n\n" +
                $"Best regards,\nThe Excellence Committee"
            );

            // שליחת התראה לראש הפקולטה
            await SendEmailAsync(
                facultyHead.Email,
                $"New Submission Requires Review - {form.FormName}",
                $"Dear {facultyHead.FirstName} {facultyHead.LastName},\n\n" +
                $"A new submission for {form.FormName} requires your review.\n\n" +
                $"Instructor: {person.FirstName} {person.LastName}\n" +
                $"Submission Date: {instance.SubmissionDate}\n" +
                $"Status: {instance.CurrentStage}\n\n" +
                $"Please login to the system to review the submission.\n\n" +
                $"Best regards,\nThe Excellence Committee"
            );
        }

        public async Task SendStatusChangeNotificationAsync(int instanceId, string oldStatus, string newStatus)
        {
            // קבלת מופע הטופס
            var instance = _instanceRepository.GetInstanceById(instanceId);
            if (instance == null)
                throw new ArgumentException($"Instance with ID {instanceId} does not exist");

            // קבלת הטופס
            var form = _formRepository.GetFormById(instance.FormId);
            if (form == null)
                throw new ArgumentException($"Form with ID {instance.FormId} does not exist");

            // קבלת המשתמש
            var person = _personRepository.GetPersonById(instance.UserID);
            if (person == null)
                throw new ArgumentException($"Person with ID {instance.UserID} does not exist");

            string subject = $"Status Update - {form.FormName}";
            string body = $"Dear {person.FirstName} {person.LastName},\n\n" +
                         $"The status of your submission for {form.FormName} has changed from '{oldStatus}' to '{newStatus}'.\n\n";

            // הוספת מידע נוסף בהתאם לסטטוס החדש
            switch (newStatus)
            {
                case "Approved":
                    body += $"Congratulations! Your submission has been approved with a score of {instance.TotalScore}.\n\n";
                    break;
                case "Rejected":
                    body += $"We regret to inform you that your submission has been rejected. " +
                           $"Please see the comments section for more information.\n\n" +
                           $"Comments: {instance.Comments}\n\n";
                    break;
                case "ReturnedForRevision":
                    body += $"Your submission requires revision. " +
                           $"Please see the comments section for more information and submit a revised version.\n\n" +
                           $"Comments: {instance.Comments}\n\n";
                    break;
                case "UnderReview":
                    body += $"Your submission is now under review. " +
                           $"You will be notified when the review process is complete.\n\n";
                    break;
            }

            body += $"Please login to the system for more details.\n\n" +
                   $"Best regards,\nThe Excellence Committee";

            // שליחת התראה למשתמש
            await SendEmailAsync(person.Email, subject, body);
        }

        public async Task SendReminderNotificationAsync(int instanceId)
        {
            // קבלת מופע הטופס
            var instance = _instanceRepository.GetInstanceById(instanceId);
            if (instance == null)
                throw new ArgumentException($"Instance with ID {instanceId} does not exist");

            // קבלת הטופס
            var form = _formRepository.GetFormById(instance.FormId);
            if (form == null)
                throw new ArgumentException($"Form with ID {instance.FormId} does not exist");

            // קבלת המשתמש
            var person = _personRepository.GetPersonById(instance.UserID);
            if (person == null)
                throw new ArgumentException($"Person with ID {instance.UserID} does not exist");

            // שליחת תזכורת לפי מצב המופע
            string subject = $"Reminder - {form.FormName}";
            string body = $"Dear {person.FirstName} {person.LastName},\n\n";

            if (instance.CurrentStage == "Draft")
            {
                // תזכורת להגשת טיוטה
                var dueDate = form.DueDate.HasValue ? form.DueDate.Value.ToShortDateString() : "the due date";
                body += $"This is a reminder that you have a draft submission for {form.FormName} that has not been submitted yet. " +
                       $"Please complete and submit your form by {dueDate}.\n\n";
            }
            else if (instance.CurrentStage == "ReturnedForRevision")
            {
                // תזכורת לתיקון
                body += $"This is a reminder that your submission for {form.FormName} requires revision. " +
                       $"Please address the comments and resubmit your form as soon as possible.\n\n" +
                       $"Comments: {instance.Comments}\n\n";
            }

            body += $"Please login to the system to access your submission.\n\n" +
                   $"Best regards,\nThe Excellence Committee";

            // שליחת התראה למשתמש
            await SendEmailAsync(person.Email, subject, body);
        }

        private string GetResponsiblePersonForInstance(int instanceId)
        {
            // פונקציה זו אמורה לקבל את זהות האחראי על מופע הטופס
            // יש ליישם את הגישה למסד הנתונים לקבלת האחראי

            // דוגמה לערך דמה - בפועל יש לקבל את זה ממסד הנתונים
            return "123456789";
        }

        private async Task SendEmailAsync(string to, string subject, string body)
        {
            try
            {
                // קבלת הגדרות שרת הדואר מקובץ התצורה
                var smtpServer = _configuration["EmailSettings:SmtpServer"];
                var smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"]);
                var smtpUsername = _configuration["EmailSettings:Username"];
                var smtpPassword = _configuration["EmailSettings:Password"];
                var fromEmail = _configuration["EmailSettings:FromEmail"];
                var fromName = _configuration["EmailSettings:FromName"];

                // הגדרת הודעת הדואר
                var message = new MailMessage();
                message.From = new MailAddress(fromEmail, fromName);
                message.To.Add(new MailAddress(to));
                message.Subject = subject;
                message.Body = body;

                // הגדרת לקוח SMTP
                using (var client = new SmtpClient(smtpServer, smtpPort))
                {
                    client.EnableSsl = true;
                    client.Credentials = new NetworkCredential(smtpUsername, smtpPassword);

                    // שליחת ההודעה
                    await client.SendMailAsync(message);
                }
            }
            catch (Exception ex)
            {
                // לוג שגיאות
                Console.WriteLine($"Error sending email: {ex.Message}");
                // במערכת אמיתית יש לנהל לוג שגיאות מסודר
                throw;
            }
        }
    }
}