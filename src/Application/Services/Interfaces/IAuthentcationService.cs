using Domain.Entities;

namespace Application.Services.Interfaces;

public interface IAuthentcationService
{
    Task<List<string>> Register(User user, string password, string role);
    Task<List<string>> LogIn(string email, string Password);
    Task<List<string>> VerifyEmail(string userId, int code);
    Task<List<string>> SendCodeToEmailToResetPassword(string email);
    Task<List<string>> ResetPassword(string userId, string newPass);
    Task<List<string>> ResendCodeToEmail(string userId);


}
