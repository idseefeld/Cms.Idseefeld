using Cms.Idseefeld.Core.Models;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;
using Umbraco.Cms.Core.Configuration.Models;

namespace Cms.Idseefeld.Core.Services
{
    public class MailKitService : MailBaseService, IMailService
    {
        private readonly IOptionsMonitor<GlobalSettings> _globalSettings;
        private readonly ContactMailOptions _mailOptions;

        public MailKitService(IOptionsMonitor<GlobalSettings> globalSettings, IOptions<ContactMailOptions> mailOptions)
        {
            _globalSettings = globalSettings;
            _mailOptions = mailOptions.Value;
        }

        public void HandleContactData(ContactFormModel formData)
        {
            if (string.IsNullOrWhiteSpace(_mailOptions.To))
            {
                throw new Exception("No recipient configured. Check appSettings ContactMail:To");
            }
            var smtpSettings = _globalSettings.CurrentValue.Smtp;
            if (smtpSettings == null || string.IsNullOrEmpty(smtpSettings.Host))
            {
                throw new Exception("No smtp configured. Check appSettings Umbraco:CMS:Global:Smtp");
            }

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(smtpSettings.From, smtpSettings.From));
            message.To.Add(new MailboxAddress(_mailOptions.ToName ?? _mailOptions.To, _mailOptions.To));
            message.Subject = formData.Subject;
            message.Body = new TextPart("plain")
            {
                Text = $"{BuildContactBody(formData)}\n\nMailKit"
            };

            using var client = new SmtpClient();
            SendMessage(smtpSettings, message, client);
        }

        private static void SendMessage(SmtpSettings smtpSettings, MimeMessage message, SmtpClient client)
        {
            if (smtpSettings.Port > 0)
            {
                var useSsl = smtpSettings.SecureSocketOptions.ToString() == SecureSocketOptions.SslOnConnect.ToString();
                client.Connect(smtpSettings.Host, smtpSettings.Port, useSsl);
            }
            else
            {
                client.Connect(smtpSettings.Host);
            }

            if (!string.IsNullOrWhiteSpace(smtpSettings.Username))
            {
                client.Authenticate(smtpSettings.Username, smtpSettings.Password);
            }

            client.Send(message);
            client.Disconnect(true);
        }
    }
}
