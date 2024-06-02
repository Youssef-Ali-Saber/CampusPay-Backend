using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using API;
using Application.DTOs;
using Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GraduationProject.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ServicesController(IServiceService serviceService) : ControllerBase
{
    [HttpPost("Add")]
    [Authorize(Roles = "Admin")]
    [ValidateModel]
    public async Task<IActionResult> AddService([FromForm]ServiceDto service)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            await serviceService.AddServiceAsync(service, userId);
            return Created();
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }
    [HttpPut("Edit")]
    [Authorize(Roles = "Admin")]
    [ValidateModel]
    public async Task<IActionResult> EditService([FromForm][Required] int serviceId, [FromForm] ServiceDto serviceDto)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            await serviceService.EtitServiceAsync(serviceId, serviceDto, userId);
            return Ok();
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }
    [HttpDelete("Delete")]
    [Authorize(Roles = "Admin")]
    public IActionResult DeleteService([Required] int serviceId)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            serviceService.DeleteServiceAsync(serviceId, userId);
            return Ok();
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    [HttpGet("GetAll")]
    [Authorize(Roles = "Student,Admin")]
    public async Task<IActionResult> GetServices()
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var services = await serviceService.GetServicesAsync(userId);
            return Ok(services?.Select(selector =>
            new
            {
                selector.Id,
                selector.FilePath,
                selector.Name
            }
            ));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }


    [HttpGet("GetDetails")]
    [Authorize(Roles = "Student,Admin")]
    public async Task<IActionResult> ServiceDetails([Required] int serviceId)
    {
        try
        {
            return Ok(await serviceService.GetServiceDetailsAsync(serviceId));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }


    [HttpPost("Pay")]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> PayService([Required] int serviceId)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Ok(new { Balance = await serviceService.PayServiceAsync(userId, serviceId) });
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }


}
