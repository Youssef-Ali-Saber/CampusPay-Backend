using Domain.IRepositories;
using Microsoft.EntityFrameworkCore.Storage;

namespace Domain.IUnitOfWork;

public interface IUnitOfWork
{
    IUserRepository UserRepository { get; }
    ISocialRequestRepository SocialRequestRepository { get; }
    IServiceRepository ServiceRepository { get; }
    IFeedbackRepository FeedbackRepository { get; }
    ITransactionRepository TransactionRepository { get; }
    ITransformationRepository TransformationRepository { get; }
    IDepositionRepository DepositionRepository { get; }
    IDonationRepository DonationRepository { get; }
    IAppWalletRepository AppWalletRepository { get; }
    IChatRepository ChatRepository { get; }
    IConnectedUserRepository ConnectedUserRepository { get; }
    Task SaveAsync();
    Task<IDbContextTransaction> BeginTransactionAsync();

}
