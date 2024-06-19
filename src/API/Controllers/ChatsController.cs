using System.Security.Claims;
using Domain.IUnitOfWork;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

/// <summary>
/// Controller for handling chat-related actions.
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class ChatsController(IUnitOfWork unitOfWork) : ControllerBase
{
    /// <summary>
    /// Gets all chats for a moderator.
    /// </summary>
    /// <param name="type">The type of chats to retrieve.</param>
    /// <returns>Action result with the list of chats.</returns>
    [HttpGet("GetAllForModerator/{type}")]
    [Authorize(Roles = "Moderator")]
    public IActionResult AllChats(string type)
    {
        try
        {
            var chats = unitOfWork.ChatRepository.GetByFilter(m => m.Type == type, [c => c.User])
                                .Select(selector => 
                                        new 
                                        { 
                                            selector.Type,
                                            selector.User.FullName,
                                            selector.User.Id 
                                        })
                                        .ToHashSet();

            return Ok(chats);

        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    /// <summary>
    /// Gets the messages for the current user or a specified user.
    /// </summary>
    /// <param name="userId">The user ID to get messages for this user. If null, gets messages for the authorized user.</param>
    /// <returns>Action result with the list of messages.</returns>
    [HttpGet("GetMassages")]
    [Authorize]
    public IActionResult MyChat([FromQuery] string? userId)
    {
        try
        {
            userId = userId ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

            var chat = unitOfWork.ChatRepository.GetByFilter(m => m.UserId == userId)
                            .Select(selector => 
                                new 
                                {
                                    massage = selector.Massage,
                                    Date = selector.Date,
                                    type = selector.Type 
                                })
                            .OrderBy(o => o.Date);

            return Ok(chat);
               
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }
}
