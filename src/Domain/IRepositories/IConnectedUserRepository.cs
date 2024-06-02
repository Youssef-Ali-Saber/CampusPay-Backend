using Domain.IRepositories.IGenericRepository;
using Domain.Entities;

namespace Domain.IRepositories;

public interface IConnectedUserRepository : IGenericRepository<ConnectedUser>
{
}
