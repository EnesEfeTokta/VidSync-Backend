namespace VidSync.Domain.Interfaces;

public interface IEmailService
{
    Task SendEmailAsync(string toEmail, string subject, string body);
}
