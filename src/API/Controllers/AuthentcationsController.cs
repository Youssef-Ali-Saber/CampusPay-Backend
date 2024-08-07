using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Application.DTOs;
using Application.Services;
using Domain.IUnitOfWork;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;
/// <summary>
/// Controller for handling authentication-related actions.
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class AuthentcationController(UserService userService, IUnitOfWork unitOfWork, JWTHandlerService jWTHandler , EmailHandlerService emailHandlerService) : ControllerBase
{
    /// <summary>
    /// Registers a new student.
    /// </summary>
    /// <param name="studentDto">The student data transfer object.</param>
    /// <returns>Action result with the registration outcome.</returns>
    [HttpPost("StudentSignUp")]
    [ValidateModel]
    public async Task<IActionResult> StudentSignup(StudentDto studentDto)
    {
        try
        {
            var result = await userService.Register(studentDto, studentDto.Password, "Student");
            return result.Count == 3 ?
                StatusCode(int.Parse(result[2]), new
                {
                    userId = result[1],
                    message = result[0],
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

    /// <summary>
    /// Registers a new donor.
    /// </summary>
    /// <param name="donorDto">The donor data transfer object.</param>
    /// <returns>Action result with the registration outcome.</returns>
    [HttpPost("DonorSignUp")]
    [ValidateModel]
    public async Task<IActionResult> DonorSignup(UserBaseDto donorDto)
    {
        try
        {
            var result = await userService.Register(donorDto, donorDto.Password, "Donor");
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
    /// Logs in a user.
    /// </summary>
    /// <param name="Email">The user's email address.</param>
    /// <param name="Password">The user's password.</param>
    /// <returns>Action result with the login outcome.</returns>
    [HttpPost("LogIn")]
    [ValidateModel]
    public async Task<IActionResult> LogIn([FromQuery][Required][EmailAddress] string Email, [FromQuery][Required] string Password)
    {
        try
        {
            var user = unitOfWork.UserRepository.GetByFilter(u => u.Email == Email).FirstOrDefault();

            if (user is null)
                return BadRequest(
                    new
                    {
                        Error = "Invalid email or password combination. Please check your credentials and try again.",
                        status = false
                    });

            if (user.IsStoped)
                return BadRequest(
                    new
                    {
                        Error = "Your Account is Stopped",
                        status = false
                    });

            var result = await unitOfWork.UserRepository.CheckPasswordAsync(user, Password);

            if (!result)
                return BadRequest(
                    new
                    {
                        Error = "Invalid email or password combination. Please check your credentials and try again.",
                        status = false
                    });

            if (user.EmailConfirmed != false)
                return Ok(
                new
                {
                    Type = unitOfWork.UserRepository.GetRolesAsync(user).Result[0],
                    Token = await jWTHandler.GenerateToken(user),
                    message = "LogIn Done Successfly",
                    status = true
                });

            await emailHandlerService.SendCodeToEmail(user);

            return BadRequest(
                new
                {
                    userId = user.Id,
                    message = "Please verify your email first Send Verification Code Again Successfly",
                    status = false
                });
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    /// <summary>
    /// Verifies the email with the provided code.
    /// </summary>
    /// <param name="verificationCode">The verification code sent to the user's email.</param>
    /// <param name="userId">The user's ID.</param>
    /// <returns>Action result with the verification outcome.</returns>
    [HttpGet("VerificationCode")]
    public async Task<IActionResult> VerificationCode([FromQuery] int verificationCode, [FromQuery] string userId)
    {
        try
        {
            var user = await unitOfWork.UserRepository.GetByIdAsync(userId);

            if (user is null)
                return BadRequest(new { Error = "Please Register Again", status = false });

            if (user.VerificationCode != verificationCode)
                return BadRequest(new { Error = "Invalid Verification Code", status = false });

            user.VerificationCode = null;
            user.EmailConfirmed = true;
            await unitOfWork.SaveAsync();

            return Ok(new { message = "Verified", status = true });

        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    /// <summary>
    /// Resends the verification code to the user's email.
    /// </summary>
    /// <param name="userId">The user's ID.</param>
    /// <returns>Action result with the resend outcome.</returns>
    [HttpGet("ResendVerificationCode")]
    public async Task<IActionResult> ResendVerificationCode([FromQuery] string userId)
    {
        try
        {
            var user = await unitOfWork.UserRepository.GetByIdAsync(userId);
            if (user is null)
                return NotFound(new { Error = "Please Register Again", status = false });
            await emailHandlerService.SendCodeToEmail(user);
            return Ok(new { message = "Send Verification Code Again Successfly", status = true });
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    /// <summary>
    /// Sends a password reset code to the user's email.
    /// </summary>
    /// <param name="email">The user's email address.</param>
    /// <returns>Action result with the outcome of the operation.</returns>
    [HttpPost("ForgetPassword")]
    [ValidateModel]
    public async Task<IActionResult> ForgetPassword([EmailAddress][FromQuery] string email)
    {
        try
        {
            var user = unitOfWork.UserRepository.GetByFilter(u => u.Email == email).FirstOrDefault();
            if (user is null)
            {
                return NotFound(new { Error = "Email Not Found", status = false });
            }
            await emailHandlerService.SendCodeToEmail(user);
            user.EmailConfirmed = false;
            await unitOfWork.SaveAsync();
            return Ok(new { userId = user.Id, message = "Send Verification Code Successfly ", status = true });
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    /// <summary>
    /// Resets the user's password.
    /// </summary>
    /// <param name="newPassword">The new password.</param>
    /// <param name="userId">The user's ID.</param>
    /// <returns>Action result with the outcome of the password reset.</returns>
    [HttpPost("ResetPassword")]
    [ValidateModel]
    public async Task<IActionResult> ResetPassword([RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,15}$")][FromQuery] string newPassword, string userId)
    {
        var user = await unitOfWork.UserRepository.GetByIdAsync(userId);
        if (user is null)
            return NotFound(new { error = "Please Register First", status = false });

        if (user.EmailConfirmed == false)
            return BadRequest(new { userId = user.Id, message = "Please verify your email first", status = false });

        await using var transaction = await unitOfWork.BeginTransactionAsync();
        try
        {
            var result = await unitOfWork.UserRepository.RemovePasswordAsync(user);
            if (!result.Succeeded)
                return BadRequest(new { error = "Failed Try Again", status = false });

            result = await unitOfWork.UserRepository.AddPasswordAsync(user, newPassword);

            if (!result.Succeeded)
                return BadRequest(new { error = "Failed Try Again", status = false });

            await unitOfWork.SaveAsync();
            await transaction.CommitAsync();
            return Ok(new { message = "Update Password Successflu", status = true });

        }
        catch (Exception e)
        {
            await transaction.RollbackAsync();
            return StatusCode(500, e.Message);
        }
    }

    /// <summary>
    /// Deletes the user account (Just used when we develop).
    /// </summary>
    /// <returns>Action result with the outcome of the account deletion.</returns>
    [HttpDelete("DeleteAccount")]
    [Microsoft.AspNetCore.Authorization.Authorize]
    public async Task<IActionResult> DeleteAccount()
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await unitOfWork.UserRepository.GetByIdAsync(userId);
            if (user == null)
                return NotFound(new { status = false });
            await unitOfWork.UserRepository.DeleteAsync<string>(userId);
            return Ok(new { status = true });
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }
}

