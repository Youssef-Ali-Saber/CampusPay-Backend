using Domain.Entities;

namespace Application.Services.Interfaces;

public interface IAccountService
{
    Task<object> GetAccounts(string type);
    object GetAccountDetails(string userId);
    Task CreateAccount(User user, string Password, string type);
    Task StopAccount(string userId);
}

