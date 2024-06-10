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
    public async Task<IActionResult> AddService([FromQuery]ServiceDto service)
    {
        try
        {
            await serviceService.AddServiceAsync(service);
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
    public async Task<IActionResult> EditService([FromQuery][Required] int serviceId, [FromQuery] ServiceDto serviceDto)
    {
        try
        {
            var service = await serviceService.GetServiceDetailsAsync(serviceId);
            if (service is null)
                return NotFound();
            await serviceService.EtitServiceAsync(serviceId, serviceDto);
            return Ok();
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }
    [HttpDelete("Delete")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteService([Required] int serviceId)
    {
        try
        {
            var service = await serviceService.GetServiceDetailsAsync(serviceId);
            if (service is null)
                return NotFound();
            await serviceService.DeleteServiceAsync(serviceId);
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
            var service = await serviceService.GetServiceDetailsAsync(serviceId);
            if (service is null)
                return NotFound();
            return Ok(service);
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
            var service = await serviceService.GetServiceDetailsAsync(serviceId);
            if (service is null)
                return NotFound();
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Ok(new { Balance = await serviceService.PayServiceAsync(userId, serviceId) });
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }


}
