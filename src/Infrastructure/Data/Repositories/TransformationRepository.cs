using Domain.Entities;
using Domain.IRepositories;
using Infrastructure.Data.Context;
using Infrastructure.Data.Repositories.GenericRepository;

namespace Infrastructure.Data.Repositories;

public class TransformationRepository(AppDbContext db) : GenericRepository<Transformation>(db) ,ITransformationRepository
{
}
