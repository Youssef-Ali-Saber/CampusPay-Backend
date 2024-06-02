using Domain.Entities;
using Domain.IRepositories;
using Infrastructure.Data.Context;
using Infrastructure.Data.Repositories.GenericRepository;

namespace Infrastructure.Data.Repositories;

public class TransactionRepository(AppDbContext db) :GenericRepository<Transaction>(db) ,ITransactionRepository

{
}
