using Application.Services.Interfaces;
using Domain.Entities;
using Domain.IRepositories;

namespace Application.Services.Implementations;

public class ConnectedUsersService(IUnitOfWork unitOfWork) : IConnectedUsersService
{

    public async Task Add(string userId, string connectionId)
    {
        var connectedUser = new ConnectedUser()
        {
            userId = userId,
            ConnectionId = connectionId
        };
        await unitOfWork.ConnectedUserRepository.CreateAsync(connectedUser);
        await unitOfWork.SaveAsync();
    }

    public async Task Remove(string connectionId)
    {
        var connectedUser = unitOfWork.ConnectedUserRepository.GetByFilter(m => m.ConnectionId == connectionId).FirstOrDefault();
        await unitOfWork.ConnectedUserRepository.DeleteAsync(connectedUser);
        await unitOfWork.SaveAsync();
    }

    public List<string> GetConnectionIds(string userId)
    {
        var connectionIds = unitOfWork.ConnectedUserRepository.GetByFilter(m => m.userId == userId).Select(n => n.ConnectionId).ToList();
        return connectionIds;
    }
}