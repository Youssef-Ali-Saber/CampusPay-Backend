namespace Application.Services.Interfaces;

public interface IChatService
{
    object GetMessages(string userId);
    Task SaveMessage(string userId, string massage, string? To_userId = null);
    Task<List<List<string>>?> GetAllBy(string? userId = null, string? type = null);
}
