using Application.Services;
using Domain.Entities;
using Domain.IUnitOfWork;
using Microsoft.AspNetCore.SignalR;

namespace API.Hubs;
public class ChatHub(ChatService chatService,IUnitOfWork unitOfWork) : Hub
{
	private string? _userId;
	private string? _role;

	public async Task SendMessageToUser(string message , string TouserId) 
	{
        var connectionIds = unitOfWork.ConnectedUserRepository.GetByFilter(m => m.userId == _userId).Select(n => n.ConnectionId).ToList();
        foreach (var connectionId in connectionIds)
		{
			await Clients.Client(connectionId).SendAsync("newMessage", message, DateTime.Now, _role);
		}
        await chatService.SaveMessageForModerator(TouserId, message);
	}


	public async Task SendMessageToModerators(string message) 
	{
		await Clients.Group("Moderators").SendAsync("newMessage", message, DateTime.Now, _role);
		await chatService.SaveMessage(_userId, message);
	}


	public override async Task OnConnectedAsync()
	{
        _role = Context.GetHttpContext()?.Request.Headers["role"];
        _userId = Context.GetHttpContext()?.Request.Headers["userId"];
		if (_role == "Moderator")
			await Groups.AddToGroupAsync(Context.ConnectionId, "Moderators");
        var connectedUser = new ConnectedUser()
        {
            userId = _userId,
            ConnectionId = Context.ConnectionId
        };
        await unitOfWork.ConnectedUserRepository.CreateAsync(connectedUser);
        await unitOfWork.SaveAsync();
    }
	
	public override async Task OnDisconnectedAsync(Exception? exception)
	{
        var connectedUser = unitOfWork.ConnectedUserRepository.GetByFilter(m => m.ConnectionId == Context.ConnectionId).FirstOrDefault();
        await unitOfWork.ConnectedUserRepository.DeleteAsync(connectedUser);
        await unitOfWork.SaveAsync();
    }

}