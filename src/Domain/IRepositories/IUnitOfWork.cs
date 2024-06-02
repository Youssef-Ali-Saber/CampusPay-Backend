using Microsoft.EntityFrameworkCore.Storage;

namespace Domain.IRepositories;

public interface IUnitOfWork
{
    IUserRepository UserRepository { get; }
    ISocialRequestRepository SocialRequestRepository { get; }
    IServiceRepository ServiceRepository { get; }
    IFeedbackRepository FeedbackRepository { get; }
    ITransactionRepository TransactionRepository { get; }
    ITransformationRepository TransformationRepository { get; }
    IServiceManagementRepository ServiceManagementRepository { get; }
    IDepositionRepository DepositionRepository { get; }
    IDonationRepository DonationRepository { get; }
    IAppWalletRepository AppWalletRepository { get; }
    IChatRepository ChatRepository { get; }
    IConnectedUserRepository ConnectedUserRepository { get; }
    Task SaveAsync();
    Task<IDbContextTransaction> BeginTransactionAsync();

}
