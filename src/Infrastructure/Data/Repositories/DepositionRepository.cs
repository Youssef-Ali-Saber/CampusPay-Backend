using Domain.Entities;
using Domain.IRepositories;
using Infrastructure.Data.Context;
using Infrastructure.Data.Repositories.GenericRepository;

namespace Infrastructure.Data.Repositories;

internal class DepositionRepository(AppDbContext db) :GenericRepository<Deposition>(db), IDepositionRepository
{
}
