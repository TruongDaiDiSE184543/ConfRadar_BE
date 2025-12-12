using Microsoft.Extensions.Options;
using MimeKit;
using System.Text;
using static ConfRadar.Services.Common.AppSettingConfig;

namespace ConfRadar.Services.Services
{
    public interface IEmailService
    {

        Task SendEmailAsync(string toEmail, string subject, string body);
        Task SendAuthenticationTemplateEmailAsync(string toEmail, string userName, string link, string subject, string templateFileName);
        Task SendCreateAccountEmail(string toEmail, string userName, string link, string subject, string templateFileName);
        Task SendSuspendTemplateEmailAsync(string toEmail, string userName, string reason, string subject, string templateFileName);
        Task SendDecidePaperAcceptedTemplateEmailAsync(string toEmail, string userName,string subject,string paperId,string coAuthor,string submissionDate,string decideReason,string
            nextPhaseStartDate, string nextPhaseEndDate, string nextPhaseDescription, string templateFileName);
        Task SendDecidePaperRejectedTemplateEmailAsync(string toEmail, string userName, string subject, string paperId, string coAuthor, string submissionDate, string decideReason, string templateFileName);
    }
    public class SmtpEmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;
        public SmtpEmailService(IOptions<EmailSettings> emailSetting)
        {
            _emailSettings = emailSetting.Value;
        }
        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {

            var email = new MimeMessage();
            email.From.Add(new MailboxAddress(Encoding.UTF8, _emailSettings.FromName, _emailSettings.FromEmail));
            email.To.Add(MailboxAddress.Parse(toEmail));
            email.Subject = subject;
            email.Body = new TextPart(MimeKit.Text.TextFormat.Html)
            {
                Text = body
            };
            using var smtp = new MailKit.Net.Smtp.SmtpClient();
            try
            {
                await smtp.ConnectAsync(_emailSettings.SmtpServer, _emailSettings.Port, MailKit.Security.SecureSocketOptions.SslOnConnect);
                smtp.Authenticate(_emailSettings.Username, _emailSettings.Password);
                await smtp.SendAsync(email);
            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {
                await smtp.DisconnectAsync(true);
            }
        }
        private string LoadTemplate(string templatePath, Dictionary<string, string> replacements)
        {
            string html = File.ReadAllText(templatePath);
            foreach (var pair in replacements)
            {
                html = html.Replace(pair.Key, pair.Value);
            }
            return html;
        }

        public async Task SendAuthenticationTemplateEmailAsync(string toEmail, string userName, string link, string subject, string templateFileName)
        {
            var replacements = new Dictionary<string, string>
    {
        { "{{UserName}}", userName },
        { "{{Link}}", link }
    };
            var templatePath = Path.Combine(AppContext.BaseDirectory, "Templates", templateFileName);
            string body = LoadTemplate(templatePath, replacements);

            await SendEmailAsync(toEmail, subject, body);
        }

        public async Task SendCreateAccountEmail(string toEmail, string userName, string link, string subject, string templateFileName)
        {
            var replacements = new Dictionary<string, string>
    {
        { "{{UserName}}", userName },
        {"{{Email}}",toEmail },
        {"{{Link}}", link }
    };
            var templatePath = Path.Combine(AppContext.BaseDirectory, "Templates", templateFileName);
            string body = LoadTemplate(templatePath, replacements);
            await SendEmailAsync(toEmail, subject, body);
        }

        public async Task SendSuspendTemplateEmailAsync(string toEmail, string userName, string reason, string subject, string templateFileName)
        {
            var replacements = new Dictionary<string, string>
    {
        { "{{UserName}}", userName },
                {"{{Reason}}", reason }
    };
            var templatePath = Path.Combine(AppContext.BaseDirectory, "Templates", templateFileName);
            string body = LoadTemplate(templatePath, replacements);
            await SendEmailAsync(toEmail, subject, body);
        }

        public async Task SendDecidePaperAcceptedTemplateEmailAsync(string toEmail, string userName, string subject, string paperId, string coAuthor, string submissionDate, string decideReason, string nextPhaseStartDate, string nextPhaseEndDate, string nextPhaseDescription, string templateFileName)
        {
            var replacements = new Dictionary<string, string>
    {
        { "{{AuthorName}}", userName },
        { "{{PaperId}}", paperId },
        { "{{CoAuthor}}", coAuthor },
        { "{{SubmissionDate}}", submissionDate },
        { "{{Reason}}", decideReason },
        { "{{NextPhaseStartDate}}", nextPhaseStartDate },
        { "{{NextPhaseEndDate}}", nextPhaseEndDate },
        { "{{NextPhaseDescription}}", nextPhaseDescription }
    };

            var templatePath = Path.Combine(AppContext.BaseDirectory, "Templates", templateFileName);
            string body = LoadTemplate(templatePath, replacements);

            await SendEmailAsync(toEmail, subject, body);
        }

        public async Task SendDecidePaperRejectedTemplateEmailAsync(string toEmail, string userName, string subject, string paperId, string coAuthor, string submissionDate, string decideReason, string templateFileName)
        {
            var replacements = new Dictionary<string, string>
    {
        { "{{AuthorName}}", userName },
        { "{{PaperId}}", paperId },
        { "{{CoAuthor}}", coAuthor },
        { "{{SubmissionDate}}", submissionDate },
        { "{{Reason}}", decideReason }
    };

            var templatePath = Path.Combine(AppContext.BaseDirectory, "Templates", templateFileName);
            string body = LoadTemplate(templatePath, replacements);

            await SendEmailAsync(toEmail, subject, body);
        }
    }
}
