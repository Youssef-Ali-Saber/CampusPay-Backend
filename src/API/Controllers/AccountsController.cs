using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Application.Services.Interfaces;
using Application.DTOs;
using Domain.Entities;

namespace GraduationProject.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Admin")]
public class AccountsController(IAccountService accountService) : ControllerBase
{

    [HttpGet("GetAll")]
    public async Task<IActionResult> GetAccounts(string type)
    {
        try
        {
            var users = await accountService.GetAccounts(type);
            return Ok(users);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }
    [HttpGet("GetDetails")]
    public IActionResult GetAccountDetails(string userId)
    {
        try
        {
            var users = accountService.GetAccountDetails(userId);
            return Ok(users);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }
    [HttpPost("Create")]
    public async Task<IActionResult> CreateAccount(UserBaseDto userDto, string type)
    {
        try
        {
            var user = new User
            {
                Email = userDto.Email,
                UserName = userDto.Email,
                FullName = userDto.FullName,
                SSN = userDto.SSN,
                Picture = userDto.Picture
            };
            await accountService.CreateAccount(user, userDto.Password, type);
            return Created();
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }
    [HttpPut("Stop")]
    public async Task<IActionResult> StopUserAccount(string userId)
    {
        try
        {
            await accountService.StopAccount(userId);
            return Ok();
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }
}
