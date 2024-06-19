
using Domain.Entities;
using Domain.IUnitOfWork;

namespace Application.Services;

public class ChatService(IUnitOfWork unitOfWork) 
{
    public async Task SaveMessage(string userId, string massage)
    {
        var user = await unitOfWork.UserRepository.GetByIdAsync(userId);
        var chat = new Chat
        {
            UserId = userId,
            Massage = massage,
            Type = unitOfWork.UserRepository.GetRolesAsync(user).Result[0],
            Date = DateTime.UtcNow
        };
        await unitOfWork.ChatRepository.CreateAsync(chat);
        await unitOfWork.SaveAsync();
    }

    public async Task SaveMessageForModerator(string To_userId , string massage)
    {
        var chat = new Chat
        {
            UserId = To_userId,
            Massage = massage,
            Type = "Moderator",
            Date = DateTime.UtcNow
        };
        await unitOfWork.ChatRepository.CreateAsync(chat);
        await unitOfWork.SaveAsync();
    }


}
