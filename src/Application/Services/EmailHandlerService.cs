using DnsClient;
using SmtpClient = MailKit.Net.Smtp.SmtpClient;
using MailKit.Security;
using MimeKit;
using System.Net.Mail;
using Domain.Entities;
using Domain.IUnitOfWork;

namespace Application.Services;

public class EmailHandlerService(IUnitOfWork unitOfWork)
{

    public async Task SendCodeToEmail(User user)
    {
        var verificationCode = _generateVerificationCode();
        _saveVerificationCode(user, verificationCode);
        await SendToEmail(user.Email, "Campus.Pay Verification Code", $"Your verification code is {verificationCode}");
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


    public static bool IsValidDomain(string email)
    {
        try
        {
            var emailChecked = new MailAddress(email);

            var domain = emailChecked.Host;
            if (CheckDomainHasMxRecord(domain))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        catch (FormatException)
        {
            return false;
        }

        bool CheckDomainHasMxRecord(string domain)
        {
            var lookup = new LookupClient();
            var result = lookup.Query(domain, QueryType.MX);
            return result.Answers.MxRecords().Count() > 0;
        }
    }


    private int _generateVerificationCode()
    {
        var rnd = new Random();
        return rnd.Next(100000, 999999);
    }


    private void _saveVerificationCode(User user, int verificationCode)
    {
        user.VerificationCode = verificationCode;
        unitOfWork.SaveAsync();
    }

}
