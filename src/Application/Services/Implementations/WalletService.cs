using Application.Services.Interfaces;
using Domain.Entities;
using Domain.IRepositories;

namespace Application.Services.Implementations;

public class WalletService(IUnitOfWork unitOfWork) : IWalletService
{
    public async Task<decimal?> DepositAsync(string userId, decimal balance, string? sessionId)
    {
        var user = await unitOfWork.UserRepository.GetByIdAsync(userId);
        using (var transaction = await unitOfWork.BeginTransactionAsync())
        {
            try
            {
                user.Balance += balance;
                var deposit = new Deposition
                {
                    Balance = balance,
                    Date = DateTime.UtcNow,
                    UserId = userId,
                    SessionId = sessionId
                };
                await unitOfWork.DepositionRepository.CreateAsync(deposit);
                await unitOfWork.SaveAsync();
                transaction.Commit();
                return user.Balance;

            }
            catch (Exception e)
            {
                transaction.Rollback();
                throw new Exception(e.Message);
            }
        }
    }

    public async Task<object?> TransferAsync(string FromUserId, string ToUserSSN, decimal balance, double? longit = null, double? latit = null)
    {

        using (var transaction = await unitOfWork.BeginTransactionAsync())
        {
            try
            {
                var FromUser = await unitOfWork.UserRepository.GetByIdAsync(FromUserId);
                var ToUser = unitOfWork.UserRepository.GetByFilter(u => u.SSN == ToUserSSN).FirstOrDefault();
                if (FromUser?.Balance < balance)
                {
                    return "Your balance not enough";
                }
                if (ToUser == null)
                {
                    return "Not found user have this SSN";
                }
                if (FromUser == ToUser)
                {
                    return "You can't transfer money to yourself";
                }
                ToUser.Balance += balance;
                FromUser.Balance -= balance;
                var transformation = new Transformation
                {
                    Balance = balance,
                    Date = DateTime.UtcNow,
                    FromUserId = FromUser.Id,
                    ToUserId = ToUser.Id,
                    Longitude = longit,
                    Latitude = latit
                };
                await unitOfWork.TransformationRepository.CreateAsync(transformation);
                await unitOfWork.SaveAsync();
                transaction.Commit();
                return FromUser.Balance;
            }
            catch
            {
                transaction.Rollback();
                return null;
            }
        }
    }

}
