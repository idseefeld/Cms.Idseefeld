using Cms.Idseefeld.Core.Models;

namespace Cms.Idseefeld.Core.Services
{
    public interface IMailService
    {
        Task HandleContactDataAsync(ContactFormModel formData);
    }
}