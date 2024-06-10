using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using API;
using Application.DTOs;
using Application.Services.Interfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace GraduationProject.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthentcationController(IAuthentcationService authorizationService, UserManager<User> userManager) : ControllerBase
{
    [HttpPost("StudentSignUp")]
    [ValidateModel]
    public async Task<IActionResult> StudentSignup(StudentDto studentDto)
    {
        try
        {
            var student = new User
            {
                UserName = studentDto.FullName.Replace(" ", "").ToLower(),
                FullName = studentDto.FullName,
                Email = studentDto.Email,
                SSN = studentDto.SSN,
                Picture = studentDto.Picture
            };
            var result = await authorizationService.Register(student, studentDto.Password, "Student");
            return result.Count == 3 ?
                StatusCode(int.Parse(result[2]), new
                {
                    userId = result[1],
                    massage = result[0],
                    status = true
                })
                : StatusCode(int.Parse(result[1]), new
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

    [HttpPost("DonorSignUp")]
    [ValidateModel]
    public async Task<IActionResult> DonorSignup(UserBaseDto donorDto)
    {
        try
        {
            var donor = new User
            {
                UserName = donorDto.FullName.Replace(" ", "").ToLower(),
                FullName = donorDto.FullName,
                Email = donorDto.Email,
                SSN = donorDto.SSN,
                Picture = donorDto.Picture
            };
            var result = await authorizationService.Register(donor, donorDto.Password, "Donor");
            if (result.Count == 3)
            {
                return StatusCode(int.Parse(result[2]), new
                {
                    userId = result[1],
                    massage = result[0],
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


    [HttpPost("LogIn")]
    [ValidateModel]
    public async Task<IActionResult> LogIn([FromQuery][Required][EmailAddress] string Email , [FromQuery][Required] string Password)
    {
        try
        {
            var result = await authorizationService.LogIn(Email , Password);
            if (result.Count == 4)
            {
                return result[3] == "200" ?
                    StatusCode(int.Parse(result[3]), new
                    {
                        Type = result[2],
                        Token = result[1],
                        massage = result[0],
                        status = true
                    })
                    : StatusCode(int.Parse(result[3]), new
                    {
                        Type = result[2],
                        userId = result[1],
                        massage = result[0],
                        status = false
                    });
            }
            return result.Count == 3 ?
                StatusCode(int.Parse(result[2]), new
                {
                    userId = result[1],
                    massage = result[0],
                    status = false
                })
                : StatusCode(int.Parse(result[1]), new
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


    [HttpGet("VerificationCode")]
    public async Task<IActionResult> VerificationCode([FromQuery] int verificationCode, [FromQuery] string userId)
    {
        try
        {
            var result = await authorizationService.VerifyEmail(userId, verificationCode);

            return int.Parse(result[1]) == 200 ?
                StatusCode(int.Parse(result[1]), new
                {
                    massage = result[0],
                    status = true
                })
                :
                StatusCode(int.Parse(result[1]), new
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


    [HttpGet("ResendVerificationCode")]
    public async Task<IActionResult> ResendVerificationCode([FromQuery] string userId)
    {
        try
        {
            var result = await authorizationService.ResendCodeToEmail(userId);
            return int.Parse(result[1]) == 200 ?
                StatusCode(int.Parse(result[1]), new
                {
                    massage = result[0],
                    status = true
                })
                :
                StatusCode(int.Parse(result[1]), new
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


    [HttpPost("ForgetPassword")]
    [ValidateModel]
    public async Task<IActionResult> ForgetPassword(
        [RegularExpression(@"^[\w\.-]+@[a-zA-Z\d\.-]+\.[a-zA-Z]{2,}$")][FromQuery] string email)
    {
        try
        {
            var result = await authorizationService.SendCodeToEmailToResetPassword(email);
            return
                result.Count == 3 ?
                StatusCode(int.Parse(result[2]), new { userId = result[1], massage = result[0], status = true })
                : StatusCode(int.Parse(result[1]), new { Error = result[0], status = false });
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }


    [HttpPost("ResetPassword")]
    [ValidateModel]
    public async Task<IActionResult> ResetPassword(
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,15}$")][FromQuery] string newPassword, string userId)
    {
        try
        {
            var result = await authorizationService.ResetPassword(userId, newPassword);
            return result.Count == 3 ?
                StatusCode(int.Parse(result[2]), new { userId = result[1], massage = result[0], status = false })
                : int.Parse(result[1]) == 200 ? StatusCode(int.Parse(result[1]), new { massage = result[0], status = true })
                : StatusCode(int.Parse(result[1]), new { error = result[0], status = false });
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }
    [HttpDelete("DeleteAccount")]
    [Microsoft.AspNetCore.Authorization.Authorize]
    public async Task<IActionResult> DeleteAccount()
    {

        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return BadRequest(new { status = false });
            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
                return BadRequest(new { status = false });
            var result = await userManager.DeleteAsync(user);
            if (result.Succeeded)
                return Ok(new { status = true });
            return BadRequest(new { status = false });
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }
}
