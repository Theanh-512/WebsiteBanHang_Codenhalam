using Microsoft.AspNetCore.Identity.UI.Services;

namespace WebsiteBanHang.Services
{
    public class EmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            // For now, this is a dummy email sender.
            return Task.CompletedTask;
        }
    }
}
