using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Application.DTOs;
using Domain.IUnitOfWork;
using Application.Services;

namespace API.Controllers;

/// <summary>
/// Accounts management controller
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Admin")]
public class AccountsController(UserService userService, IUnitOfWork unitOfWork) : ControllerBase
{
    /// <summary>
    /// Get all accounts based on type
    /// </summary>
    /// <param name="type">Type of accounts to retrieve</param>
    /// <returns>List of user accounts</returns>
    [HttpGet("GetAll")]
    public async Task<IActionResult> GetAccounts(string type)
    {
        try
        {
            var users = await unitOfWork.UserRepository.GetAllInRoleAsync(type);

            var accounts = users.Select(selector => new { selector.Id, selector.FullName });

            return Ok(accounts);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    /// <summary>
    /// Get account details for a specific user
    /// </summary>
    /// <param name="userId">User ID of the account</param>
    /// <returns>User account details</returns>
    [HttpGet("GetDetails")]
    public IActionResult GetAccountDetails(string userId)
    {
        try
        {
            var acountDetails = unitOfWork.UserRepository.GetByFilter(i => i.Id == userId)
                        .Select(selector => new
                        {
                            selector.Id,
                            selector.FullName,
                            selector.Email,
                            selector.FilePath,
                            selector.SSN
                        });

            return Ok(acountDetails);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    /// <summary>
    /// Create a new user account
    /// </summary>
    /// <param name="userDto">User data transfer object containing account details</param>
    /// <param name="type">Type of account to create</param>
    /// <returns>Result of account creation</returns>
    [HttpPost("Create")]
    public async Task<IActionResult> CreateAccount(UserBaseDto userDto, string type)
    {
        try
        {
            
            var result = await userService.Register(userDto, userDto.Password, type);
            if (result.Count == 3)
            {
                return StatusCode(int.Parse(result[2]), new
                {
                    userId = result[1],
                    message = result[0],
                    status = true
                });
            }
            return StatusCode(int.Parse(result[1]), new
            {
                Error = result[0],
                status = false
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    /// <summary>
    /// Stop a user account
    /// </summary>
    /// <param name="userId">User ID of the account to stop</param>
    /// <returns>Result of the operation</returns>
    [HttpPut("Stop")]
    public async Task<IActionResult> StopUserAccount(string userId)
    {
        try
        {
            var user = unitOfWork.UserRepository.GetByFilter(i => i.Id == userId).FirstOrDefault();
            user.IsStoped = true;
            unitOfWork.UserRepository.Update(user);
            await unitOfWork.SaveAsync();
            return Ok();
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }
}
