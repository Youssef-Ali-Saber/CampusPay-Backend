using Application.Services.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace GraduationProject.Hubs;
public class ChatHub(IChatService chatService,IConnectedUsersService connectedUsersService) : Hub
{
	private string? _userId;
	private string? _role;
	public async Task SendMessageToUser(string message , string TouserId) 
	{
	    var connectionIds = connectedUsersService.GetConnectionIds(TouserId);
		foreach (var connectionId in connectionIds)
		{
			await Clients.Client(connectionId).SendAsync("newMessage", message, DateTime.Now, _role);
		}
        await chatService.SaveMessage(_userId, message , TouserId);
        
	}
	public async Task SendMessageToModerators(string message) 
	{
		await Clients.Group("Moderators").SendAsync("newMessage", message, DateTime.Now, _role);
		await chatService.SaveMessage(_userId, message);
	}
	public override async Task OnConnectedAsync()
	{
        _role = Context.GetHttpContext()?.Request.Headers["roleOfUser"];
        _userId = Context.GetHttpContext()?.Request.Headers["userId"];
		if (_role == "Moderator")
			await Groups.AddToGroupAsync(Context.ConnectionId, "Moderators");
		await connectedUsersService.Add(_userId, Context.ConnectionId);
	}
	
	public override async Task OnDisconnectedAsync(Exception? exception)
	{
		await connectedUsersService.Remove(Context.ConnectionId);
	}

}