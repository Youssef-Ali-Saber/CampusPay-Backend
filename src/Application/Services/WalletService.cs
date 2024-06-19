using Domain.Entities;
using Domain.IUnitOfWork;

namespace Application.Services;

public class WalletService(IUnitOfWork unitOfWork)
{
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
