using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Application.DTOs;
using Application.Services;
using Domain.Entities;
using Domain.IUnitOfWork;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

/// <summary>
/// Services Controller to manage service operations.
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class ServicesController(IUnitOfWork unitOfWork,FilesUploaderService filesUploader) : ControllerBase
{
    /// <summary>
    /// Adds a new service.
    /// </summary>
    /// <param name="service">Service data transfer object containing service details.</param>
    /// <returns>Action result indicating the result of the operation.</returns>
    [HttpPost("Add")]
    [Authorize(Roles = "Admin")]
    [ValidateModel]
    public async Task<IActionResult> AddService([FromQuery] ServiceDto serviceDto)
    {
        try
        {
            var addService = new Service
            {
                Cost = serviceDto.Cost,
                Description = serviceDto.Description,
                Name = serviceDto.Name,
                Type = serviceDto.Type,
                SquadYear = serviceDto.SquadYear,
                CollegeName = serviceDto.CollegeName
            };
            if (serviceDto.Icon != null)
                addService.FilePath = await filesUploader.UploadImageAsync(serviceDto.Icon, "Images/Icon/");
            await unitOfWork.ServiceRepository.CreateAsync(addService);
            await unitOfWork.SaveAsync();
            return Created();
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    /// <summary>
    /// Edits an existing service.
    /// </summary>
    /// <param name="serviceId">ID of the service to edit.</param>
    /// <param name="serviceDto">Service data transfer object containing updated service details.</param>
    /// <returns>Action result indicating the result of the operation.</returns>
    [HttpPut("Edit")]
    [Authorize(Roles = "Admin")]
    [ValidateModel]
    public async Task<IActionResult> EditService([FromQuery][Required] int serviceId, [FromQuery] ServiceDto serviceDto)
    {
        try
        {
            var service = await unitOfWork.ServiceRepository.GetByIdAsync(serviceId);
            if (service is null)
                return NotFound();
            service.Cost = serviceDto.Cost;
            service.Description = serviceDto.Description;
            service.Name = serviceDto.Name;
            service.Type = serviceDto.Type;
            service.SquadYear = serviceDto.SquadYear;
            service.CollegeName = serviceDto.CollegeName;
            if (serviceDto.Icon != null)
                service.FilePath = await filesUploader.UploadImageAsync(serviceDto.Icon, "Images/Icon/");
            unitOfWork.ServiceRepository.Update(service);
            await unitOfWork.SaveAsync();
            return Ok();
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    /// <summary>
    /// Deletes a service.
    /// </summary>
    /// <param name="serviceId">ID of the service to delete.</param>
    /// <returns>Action result indicating the result of the operation.</returns>
    [HttpDelete("Delete")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteService([Required] int serviceId)
    {
        try
        {
            var service = await unitOfWork.ServiceRepository.GetByIdAsync(serviceId);
            if (service is null)
                return NotFound();
            await unitOfWork.ServiceRepository.DeleteAsync<int>(serviceId);
            await unitOfWork.SaveAsync();
            return Ok();
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    /// <summary>
    /// Gets all services.
    /// </summary>
    /// <returns>A list of services.</returns>
    [HttpGet("GetAll")]
    [Authorize(Roles = "Student,Admin")]
    public IActionResult GetServices()
    {
        try
        {
            var services = unitOfWork.ServiceRepository.GetAll();
            return Ok(services?.Select(selector => new
            {
                selector.Id,
                selector.FilePath,
                selector.Name
            }));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    /// <summary>
    /// Gets details of a specific service.
    /// </summary>
    /// <param name="serviceId">ID of the service.</param>
    /// <returns>Service details.</returns>
    [HttpGet("GetDetails")]
    [Authorize(Roles = "Student,Admin")]
    public async Task<IActionResult> ServiceDetails([Required] int serviceId)
    {
        try
        {
            var service = await unitOfWork.ServiceRepository.GetByIdAsync(serviceId);

            if (service is null)
                return NotFound();

            return Ok(service);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    /// <summary>
    /// Pays for a service.
    /// </summary>
    /// <param name="serviceId">ID of the service to pay for.</param>
    /// <returns>Action result indicating the result of the payment.</returns>
    [HttpPost("Pay")]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> PayService([Required] int serviceId)
    {
        var service = await unitOfWork.ServiceRepository.GetByIdAsync(serviceId);
        if (service is null)
            return NotFound();
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var user = await unitOfWork.UserRepository.GetByIdAsync(userId);

        if (user.Balance < service.Cost)
            return BadRequest(new { message = "Your balance is not enough" });

        using (var transaction = await unitOfWork.BeginTransactionAsync())
        {
            try
            {
                user.Balance -= service.Cost;
                var trans = new Transaction
                {
                    Date = DateTime.UtcNow,
                    ServiceId = service.Id,
                    UserId = user.Id,
                    Status = "Done"
                };
                var ServicePayment = unitOfWork.AppWalletRepository.GetByFilter(a => a.Type == "ServicePayments").FirstOrDefault();
                if (ServicePayment is null)
                {
                    var appWallet = new AppWallet
                    {
                        Balance = service.Cost,
                        Type = "ServicePayments"
                    };
                    await unitOfWork.AppWalletRepository.CreateAsync(appWallet);
                }
                else
                {
                    ServicePayment.Balance += service.Cost;
                }

                await unitOfWork.TransactionRepository.CreateAsync(trans);
                await unitOfWork.SaveAsync();
                transaction.Commit();
                return Ok(new { Balance = user.Balance });
            }
            catch(Exception ex)
            {
                transaction.Rollback();
                return StatusCode(500, ex.Message);
            }
        }
    }
}
