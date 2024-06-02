using Domain.Entities;
using Domain.IRepositories;
using Infrastructure.Data.Context;
using Infrastructure.Data.Repositories.GenericRepository;

namespace Infrastructure.Data.Repositories;

public class FeedbackRepository(AppDbContext db) : GenericRepository<Feedback>(db) , IFeedbackRepository
{
}
