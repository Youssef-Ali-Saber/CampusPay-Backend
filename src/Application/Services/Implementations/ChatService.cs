using Application.Services.Interfaces;
using Domain.Entities;
using Domain.IRepositories;

namespace Application.Services.Implementations;

public class ChatService(IUnitOfWork unitOfWork) : IChatService
{
    public async Task SaveMessage(string userId, string massage, string? To_userId = null)
    {
        var chat = new Chat();
        if (string.IsNullOrEmpty(To_userId))
        {
            var user = await unitOfWork.UserRepository.GetByIdAsync(userId);
            chat = new Chat
            {
                UserId = userId,
                Massage = massage,
                Type = unitOfWork.UserRepository.GetRolesAsync(user).Result[0],
                Date = DateTime.UtcNow
            };
        }
        else
        {
            chat = new Chat
            {
                UserId = To_userId,
                Massage = massage,
                Type = "Moderator",
                Date = DateTime.UtcNow
            };
        }
        await unitOfWork.ChatRepository.CreateAsync(chat);
        await unitOfWork.SaveAsync();
    }

    public object GetMessages(string userId)
    {
        return unitOfWork.ChatRepository.GetByFilter(m => m.UserId == userId)
            .Select(selector => new { massage = selector.Massage, selector.Date, type = selector.Type })
            .OrderBy(o => o.Date);
    }

    public async Task<List<List<string>>?> GetAllBy(string? userId = null, string? type = null)
    {
        if (userId == null && type != null)
        {
            return unitOfWork.ChatRepository.GetByFilter(m => m.Type == type, [c => c.User])
                .Select(selector => new List<string> { selector.Type, selector.User.FullName, selector.User.Id })
                .ToHashSet().ToList();
        }

        if (type == null && userId != null)
        {
            var user = await unitOfWork.UserRepository.GetByIdAsync(userId);
            var role = unitOfWork.UserRepository.GetRolesAsync(user).Result[0];
            return unitOfWork.ChatRepository.GetByFilter(m => m.Type == role, [c => c.User])
                .Select(selector => new List<string> { selector.Type, selector.User.FullName, selector.User.Id })
                .ToHashSet().ToList();
        }

        return null;
    }
}
