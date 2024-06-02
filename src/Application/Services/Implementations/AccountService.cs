using Application.Services.Interfaces;
using Domain.Entities;
using Domain.IRepositories;
using Infrastructure.Services;

namespace Application.Services.Implementations;

public class AccountService(IUnitOfWork unitOfWork, FilesUploader filesUploader) : IAccountService
{
    #region Manage Accounts
    public async Task<object> GetAccounts(string type)
    {
        var Users = await unitOfWork.UserRepository.GetAllInRoleAsync(type);
        return Users.Select(selector => new { selector.Id, selector.FullName });
    }
    public object GetAccountDetails(string userId)
    {
        return unitOfWork.UserRepository.GetByFilter(i => i.Id == userId)
                        .Select(selector => new
                        {
                            selector.Id,
                            selector.FullName,
                            selector.Email,
                            selector.FilePath,
                            selector.SSN
                        });
    }

    public async Task CreateAccount(User user, string Password, string type)
    {

        user.FilePath = await filesUploader.UploadIconAsync(user.Picture);
        await unitOfWork.UserRepository.CreateAsync(user, Password);
        await unitOfWork.UserRepository.AddToRoleAsync(user, type);
        await unitOfWork.SaveAsync();
    }

    public async Task StopAccount(string userId)
    {
        var user = unitOfWork.UserRepository.GetByFilter(i => i.Id == userId).FirstOrDefault();
        user.IsStoped = true;
        unitOfWork.UserRepository.Update(user);
        await unitOfWork.SaveAsync();
    }
    #endregion
}
