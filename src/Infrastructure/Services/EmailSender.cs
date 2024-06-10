using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace Infrastructure.Services;

public static class EmailSender
{
    
    public static async Task SendVerificationCodeToEmailAsync(string email, int verificationCode)
    {
        await SendToEmail(email, "Campus.Pay Verification Code", $"Your verification code is {verificationCode}");
    }
    public static async Task SendToEmail(string email, string subject, string message)
    {
        try
        {
            var mimeMessage = new MimeMessage();
            mimeMessage.From.Add(new MailboxAddress("Campus.Pay", "campus.pay.edu@gmail.com"));
            mimeMessage.To.Add(new MailboxAddress("", email));
            mimeMessage.Subject = subject;
            mimeMessage.Body = new TextPart("plain")
            {
                Text = message
            };

            using (var client = new SmtpClient())
            {
                await client.ConnectAsync("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
                await client.AuthenticateAsync("campus.pay.edu@gmail.com", "pfir kylr coez xhtx");
                await client.SendAsync(mimeMessage);
                await client.DisconnectAsync(true);
            }
        }
        catch
        {
            throw new Exception("Failed to send email");
        }
        
    }
}
