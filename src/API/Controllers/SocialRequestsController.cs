using API;
using Application.DTOs;
using Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GraduationProject.Controllers;

[Route("api/[controller]")]
[ApiController]
public class SocialRequestsController(ISocialRequestService socialRequestService) : ControllerBase
{


    [HttpPost("Add")]
    [ValidateModel]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> PostSocialRequest([FromQuery]SocialRequestDto socialRequest)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            await socialRequestService.CreateAsync(socialRequest, userId);
            return Created();
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    [HttpGet("GetAll")]
    [Authorize(Roles = "Student,Donor,Moderator")]
    public IActionResult GetAll()
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Ok(socialRequestService.GetAllAsync(userId).Result.Select(selector =>
        new
        {
            selector.Service.Name,
            selector.Service.Cost,
            selector.Service.Type,
            selector.Status
        }
        ));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    [HttpGet("Get_Details")]
    [Authorize(Roles = "Student,Donor,Moderator")]
    public IActionResult GetById(int socialRequestId)
    {
        try
        {
            return Ok(socialRequestService.GetDetailsAsync(socialRequestId));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }


    [HttpPut("Accept")]
    [Authorize(Roles = "Moderator")]
    public IActionResult Accept(int id, bool status)
    {
        try
        {
            socialRequestService.AcceptAsync(id, status);
            return Ok();
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }


    [HttpPut("Donate")]
    [Authorize(Roles = "Donor")]
    public IActionResult Donate(int socialRequestId, int visaId)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            socialRequestService.DonateAsync(socialRequestId, userId);
            return Ok();
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

}
