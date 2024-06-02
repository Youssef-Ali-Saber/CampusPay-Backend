using System.Security.Claims;
using Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GraduationProject.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ChatsController(IChatService chatService) : ControllerBase
{

    [HttpGet("GetAllForModerator/{type}")]
    [Authorize(Roles = "Moderator")]
    public async Task<IActionResult> AllChats(string type)
    {
        try
        {
            return Ok(await chatService.GetAllBy(type: type));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    [HttpGet("GetMassagesOf/{userId}")]
    [Authorize]
    public IActionResult MyChat(string? userId)
    {
        try
        {
            var userIdOfAuthorizeUser = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Ok(
                userId == null ?
                    chatService.GetMessages(userIdOfAuthorizeUser)
                    : chatService.GetMessages(userId)
                );
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }


}
