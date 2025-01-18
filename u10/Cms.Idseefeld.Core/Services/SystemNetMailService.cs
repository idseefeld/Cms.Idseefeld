using Cms.Idseefeld.Core.Models;
using Microsoft.Extensions.Options;
using System.Net.Mail;
using Umbraco.Cms.Core.Configuration.Models;

namespace Cms.Idseefeld.Core.Services
{
    public class SystemNetMailService : MailBaseService, IMailService
    {
        private readonly IOptionsMonitor<GlobalSettings> _globalSettings;
        private readonly ContactMailOptions _mailOptions;

        public SystemNetMailService(IOptionsMonitor<GlobalSettings> globalSettings, IOptions<ContactMailOptions> mailOptions)
        {
            _globalSettings = globalSettings;
            _mailOptions = mailOptions.Value;
        }
        public void HandleContactData(ContactFormModel formData)
        {
            if (!formData.DataConsent) return;

            if (string.IsNullOrWhiteSpace(_mailOptions.To))
            {
                throw new Exception("No recipient configured. Check appSettings ContactMail:To");
            }
            var smtpSettings = _globalSettings.CurrentValue.Smtp;
            if (smtpSettings == null || string.IsNullOrEmpty(smtpSettings.Host))
            {
                throw new Exception("No smtp configured. Check appSettings Umbraco:CMS:Global:Smtp");
            }

            var message = new MailMessage(smtpSettings.From, _mailOptions.To, formData.Subject, BuildContactBody(formData));

            using var client = new SmtpClient(smtpSettings.Host);
            SendMessage(smtpSettings, message, client);

        }
        private static void SendMessage(SmtpSettings smtpSettings, MailMessage message, SmtpClient client)
        {
            var useSsl = smtpSettings.SecureSocketOptions.ToString() == SecureSocketOptions.SslOnConnect.ToString();
            client.EnableSsl = useSsl;
            client.Credentials = new System.Net.NetworkCredential(smtpSettings.Username, smtpSettings.Password);

            client.Send(message);
        }
    }
}
