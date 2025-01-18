using Cms.Idseefeld.Core.Models;

namespace Cms.Idseefeld.Core.Services
{
    public interface IMailService
    {
        void HandleContactData(ContactFormModel formData);
    }
}