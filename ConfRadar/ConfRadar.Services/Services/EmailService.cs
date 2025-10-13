using Microsoft.Extensions.Options;
using MimeKit;
using System.Text;
using static ConfRadar.Services.Common.AppSettingConfig;

namespace ConfRadar.Services.Services
{
    public interface IEmailService
    {

        Task SendEmailAsync(string toEmail, string subject, string body);
        Task SendRegistrationConfirmationEmailAsync(string toEmail, string userName, string confirmLink);
    }
    public class SmtpEmailService: IEmailService
    {
        private readonly EmailSettings _emailSettings;
        public SmtpEmailService(IOptions<EmailSettings> emailSetting)
        {
            _emailSettings = emailSetting.Value;
        }
        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            
            var email = new MimeMessage();
            email.From.Add(new MailboxAddress(Encoding.UTF8,_emailSettings.FromName,_emailSettings.FromEmail));
            email.To.Add(MailboxAddress.Parse(toEmail));
            email.Subject = subject;
            email.Body = new TextPart(MimeKit.Text.TextFormat.Html)
            {
                Text = body
            };
            using var smtp = new MailKit.Net.Smtp.SmtpClient();
            try
            {
                await smtp.ConnectAsync(_emailSettings.SmtpServer,_emailSettings.Port , MailKit.Security.SecureSocketOptions.SslOnConnect);
                smtp.Authenticate(_emailSettings.Username,_emailSettings.Password );
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

        public async Task SendRegistrationConfirmationEmailAsync(string toEmail, string userName, string confirmLink)
        {
            var replacements = new Dictionary<string, string>
 {
 { "{{UserName}}", userName },
 { "{{ConfirmLink}}", confirmLink }
 };
            var templatePath = Path.Combine(AppContext.BaseDirectory, "Templates", "EmailRegistrationConfirmation.html");
            string body = LoadTemplate(templatePath, replacements);
            await SendEmailAsync(toEmail, "Confirm Account Registration", body);
        }
    }
}
