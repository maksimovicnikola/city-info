namespace CityInfo.API.Services;

public class CloudMailService : IMailService
{
    private string _mailTo = "admin@company.com";
    private string _mailFrom = "noreply@company.com";

    public void Send(string subject, string message)
    {
        // Send Email - output to console window
        Console.WriteLine($"Mail from {_mailFrom} to {_mailTo}: with {nameof(CloudMailService)}.");
        Console.WriteLine($"Subject: {subject}");
        Console.WriteLine($"Message: {message}");
    }
}