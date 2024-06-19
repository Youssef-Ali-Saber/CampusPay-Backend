using Domain.Entities;
using Domain.IRepositories;
using Domain.IUnitOfWork;
using Infrastructure.Data.Context;
using Infrastructure.Data.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Storage;

namespace Infrastructure.Data.UnitOfWork;

public class UnitOfWork : IUnitOfWork
{
    private readonly SqlServerDbContext db;
    public UnitOfWork(SqlServerDbContext db, UserManager<User> userManager)
    {
        this.db = db;
        UserRepository = new UserRepository(db, userManager);
        SocialRequestRepository = new SocialRequestRepository(db);
        ServiceRepository = new ServiceRepository(db);
        FeedbackRepository = new FeedbackRepository(db);
        TransactionRepository = new TransactionRepository(db);
        TransformationRepository = new TransformationRepository(db);
        DepositionRepository = new DepositionRepository(db);
        DonationRepository = new DonationRepository(db);
        AppWalletRepository = new AppWalletRepository(db);
        ChatRepository = new ChatRepository(db);
        ConnectedUserRepository = new ConnectedUserRepository(db);
    }
    public IUserRepository UserRepository { get; private set; }
    public IConnectedUserRepository ConnectedUserRepository { get; private set; }

    public ISocialRequestRepository SocialRequestRepository { get; private set; }

    public IServiceRepository ServiceRepository { get; private set; }

    public IFeedbackRepository FeedbackRepository { get; private set; }

    public ITransactionRepository TransactionRepository { get; private set; }

    public ITransformationRepository TransformationRepository { get; private set; }

    public IDepositionRepository DepositionRepository { get; private set; }

    public IDonationRepository DonationRepository { get; private set; }

    public IAppWalletRepository AppWalletRepository { get; private set; }

    public IChatRepository ChatRepository { get; private set; }

    public async Task<IDbContextTransaction> BeginTransactionAsync()
    {
        return await db.Database.BeginTransactionAsync();
    }
    public async Task SaveAsync()
    {
        await db.SaveChangesAsync();
    }
}
