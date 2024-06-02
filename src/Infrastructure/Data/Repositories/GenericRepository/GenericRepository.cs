using Domain.IRepositories.IGenericRepository;
using Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Infrastructure.Data.Repositories.GenericRepository;

public class GenericRepository<TEntity> : IGenericRepository<TEntity> where TEntity : class
{
    private readonly AppDbContext _dbContext;
    public DbSet<TEntity> table;
    public GenericRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
        table = dbContext.Set<TEntity>();
    }
    public async Task CreateAsync(TEntity entity)
    {
        await table.AddAsync(entity);
    }

    public async Task DeleteAsync<TKey>(TKey id)
    {
        TEntity? entity = await GetByIdAsync(id);
        if (entity != null)
        {
            table.Remove(entity);
        }
        else
        {
            throw new Exception("Entity not found");
        }
    }

    public IQueryable<TEntity> GetAll()
    {
        return table;
    }

    public IQueryable<TEntity> GetByFilter(Expression<Func<TEntity,bool>> filter)
    {
        var entities = table.Where(filter);
        return entities;
    }

    public IQueryable<TEntity> GetByFilter(Expression<Func<TEntity, bool>> filter, List<Expression<Func<TEntity, object>>> includes)
    {
        var entities = GetByFilter(filter);
        foreach (var include in includes)
        {
            entities = entities.Include(include);
        }
        return entities;
    }

    public IQueryable<TEntity> GetByFilter(Expression<Func<TEntity, bool>> filter, List<Expression<Func<TEntity, object>>> includes, List<Expression<Func<TEntity, object>>> orderBy)
    {
        var entities = GetByFilter(filter, includes);
        foreach (var order in orderBy)
        {
            entities = entities.OrderBy(order);
        }
        return entities;
    }

    public async Task<TEntity?> GetByIdAsync<TKey>(TKey id)
    {
        var entity = await table.FindAsync(id);
        return entity;
    }

    public void Update(TEntity entity)
    {
        table.Update(entity);
    }

}
