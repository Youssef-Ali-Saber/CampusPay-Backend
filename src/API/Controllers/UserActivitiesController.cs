using System.Security.Claims;
using Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GraduationProject.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UserActivitiesController(IUserActivitieService userActivitieService) : ControllerBase
{

    [HttpPut("UpdateProfile")]
    [Authorize]
    public async Task<IActionResult> UpdateProfileInfo(IFormFile? picture = null, string? newFullName = null)
    {
        try
        {

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var errors = await userActivitieService.UpdateProfileAsync(userId, newFullName, picture);
            if (!errors.TryGetValue("file", out var error))
                return Ok();
            ModelState.AddModelError("file", error);
            return BadRequest(ModelState);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }



    [HttpGet("Profile")]
    [Authorize]
    public IActionResult MyProfile()
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var profile = userActivitieService.GetProfile(e => e.Id == userId);
            return Ok(profile);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    [HttpPost("AddFeedback")]
    [Authorize(Roles = "Student,Donor")]
    public IActionResult Feedback(string feedback)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            userActivitieService.AddFeedbackAsync(feedback, userId);
            
            return Created();
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }


    [HttpGet("GetAllFeedbacks")]
    [Authorize(Roles = "Moderator")]
    public IActionResult GetAllFeedbacks()
    {
        try
        {
            return Ok(userActivitieService.GetFeedbacks());
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }


    [HttpGet("HistoryOfTransactions")]
    [Authorize(Roles = "Student")]
    public IActionResult HistoryOfTransaction()
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Ok(userActivitieService.GetHistoryTransactions(userId));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }


    [HttpGet("HistoryOfDonations")]
    [Authorize(Roles = "Donor")]
    public IActionResult HistoryOfDonations()
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Ok(userActivitieService.HistoryDonations(userId));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

}
