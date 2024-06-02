namespace Application.Services.Interfaces;
public interface IConnectedUsersService
{
    Task Add(string userId, string connectionId);
    Task Remove(string connectionId);
    List<string> GetConnectionIds(string userId);
}