using System.Security.Claims;
using Application.Services;
using Domain.Entities;
using Domain.IUnitOfWork;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace API.Controllers;

/// <summary>
/// Controller to manage user activities.
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class UserActivitiesController(UserService userService, IUnitOfWork unitOfWork ,FilesUploaderService filesUploader) : ControllerBase
{
    /// <summary>
    /// Updates the user's profile information.
    /// </summary>
    /// <param name="picture">The new profile picture (optional).</param>
    /// <param name="newFullName">The new full name (optional).</param>
    /// <returns>Action result indicating the result of the operation.</returns>
    [HttpPut("UpdateProfile")]
    [Authorize]
    public async Task<IActionResult> UpdateProfileInfo(IFormFile? picture = null, string? newFullName = null)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await unitOfWork.UserRepository.GetByIdAsync(userId);

            if (!newFullName.IsNullOrEmpty())
                user.FullName = newFullName;
            Dictionary<string, string> errors = new();

            if (picture != null)
            {
                errors = filesUploader.ValidateFile(picture);
                if (errors.IsNullOrEmpty())
                {
                    user.FilePath = await filesUploader.UploadImageAsync(picture, "Images/Picture/");
                }
            }
            unitOfWork.UserRepository.Update(user);
            await unitOfWork.SaveAsync();

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

    /// <summary>
    /// Retrieves the current user's profile information.
    /// </summary>
    /// <returns>The profile information of the current user.</returns>
    [HttpGet("Profile")]
    [Authorize]
    public async Task<IActionResult> MyProfile()
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await unitOfWork.UserRepository.GetByIdAsync(userId);
            var profile = new
            {
                picture = user.FilePath,
                email = user.Email,
                fullName = user.FullName,
                ssn = user.SSN,
                userId = user.Id
            };
            return Ok(profile);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    /// <summary>
    /// Adds feedback from the user.
    /// </summary>
    /// <param name="feedback">The feedback content.</param>
    /// <returns>Action result indicating the result of the operation.</returns>
    [HttpPost("AddFeedback")]
    [Authorize(Roles = "Student,Donor")]
    public async Task<IActionResult> Feedback(string feedback)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var newFeedback = new Feedback
            {
                FeedBackMassage = feedback,
                FeedBack_DateTime = DateTime.UtcNow,
                UserId = userId
            };
            await unitOfWork.FeedbackRepository.CreateAsync(newFeedback);
            await unitOfWork.SaveAsync();
            return Created();
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    /// <summary>
    /// Retrieves all feedbacks.
    /// </summary>
    /// <returns>A list of all feedbacks.</returns>
    [HttpGet("GetAllFeedbacks")]
    [Authorize(Roles = "Moderator")]
    public IActionResult GetAllFeedbacks()
    {
        try
        {
            var feedbacks = unitOfWork.FeedbackRepository.GetAll()
                                .Include(u => u.User)
                                .Select(s => new
                                {
                                    feedBackDate = s.FeedBack_DateTime,
                                    feedBackMassage = s.FeedBackMassage,
                                    fullName = s.User.FullName,
                                    collegeName = s.User.College_Name
                                }).ToList();
            return Ok(feedbacks);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    /// <summary>
    /// Retrieves the transaction history of the current student user.
    /// </summary>
    /// <returns>A list of transaction history.</returns>
    [HttpGet("HistoryOfTransactions")]
    [Authorize(Roles = "Student")]
    public IActionResult HistoryOfTransaction()
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var historyTransactions = new
            {
                transactions = unitOfWork.TransactionRepository.GetByFilter(t => t.UserId == userId, [s => s.Service])
                                .Select(s => new
                                {
                                    date = DateOnly.FromDateTime(s.Date),
                                    time = $"{s.Date.Hour}:{s.Date.Minute}",
                                    serviceName = s.Service.Name,
                                    cost = s.Service.Cost,
                                    serviceType = s.Service.Type

                                }).ToList(),

                deposits = unitOfWork.DepositionRepository.GetByFilter(d => d.UserId == userId)
                                .Select(s => new
                                {
                                    date = DateOnly.FromDateTime(s.Date),
                                    time = $"{s.Date.Hour}:{s.Date.Minute}",
                                    balance = s.Balance
                                }).ToList(),

                transferFromMe = unitOfWork.TransformationRepository.GetByFilter(t => t.FromUserId == userId)
                                .Select(s => new
                                {
                                    balance = s.Balance,
                                    date = DateOnly.FromDateTime(s.Date),
                                    time = $"{s.Date.Hour}:{s.Date.Minute}",
                                    FromUserFullName = s.FromUser.FullName,
                                    ToUserFullName = unitOfWork.UserRepository.GetByIdAsync(s.ToUserId).Result.FullName


                                }).ToList(),

                transferToMe = unitOfWork.TransformationRepository.GetByFilter(t => t.ToUserId == userId)
                                .Select(s => new
                                {
                                    balance = s.Balance,
                                    date = DateOnly.FromDateTime(s.Date),
                                    time = $"{s.Date.Hour}:{s.Date.Minute}",
                                    FromUserFullName = s.FromUser.FullName,
                                    ToUserFullName = unitOfWork.UserRepository.GetByIdAsync(s.ToUserId).Result.FullName
                                }).ToList()
            };
            return Ok(historyTransactions);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    /// <summary>
    /// Retrieves the donation history of the current donor user.
    /// </summary>
    /// <returns>A list of donation history.</returns>
    [HttpGet("HistoryOfDonations")]
    [Authorize(Roles = "Donor")]
    public IActionResult HistoryOfDonations()
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var historyOfDonations = unitOfWork.DonationRepository.GetByFilter(m => m.UserId == userId, [i => i.User, i => i.SocialRequest])
                                                .Select(s => new
                                                {
                                                    Service_Name = s.SocialRequest.Service.Name,
                                                    amount = s.SocialRequest.Service.Cost,
                                                    Student_Name = s.User.FullName.Split()[0],
                                                    college = s.User.College_Name,
                                                    squadYear = s.User.Squad_Year,
                                                    date = DateOnly.FromDateTime(s.Date),
                                                    time = $"{s.Date.Hour}:{s.Date.Minute}"
                                                });
            return Ok(historyOfDonations);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    /// <summary>
    /// Retrieves the total amount of money paid by the current student user.
    /// </summary>
    /// <returns>The total amount of money paid.</returns>
    [HttpGet("TotelOfMoneyPayed")]
    [Authorize(Roles = "Student")]
    public IActionResult TotelOfMoneyPayed()
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var transactions = unitOfWork.TransactionRepository.GetByFilter(t => t.UserId == userId, [m => m.Service]);

            var totalOfMoneyPayed = transactions?.Sum(s => s.Service.Cost);

            var transferFromMe = unitOfWork.TransformationRepository.GetByFilter(t => t.FromUserId == userId);

            var total = totalOfMoneyPayed + transferFromMe.Sum(s => s.Balance);

            return Ok(new { Balance = total });
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    /// <summary>
    /// Retrieves the total amount of money deposited by the current student user.
    /// </summary>
    /// <returns>The total amount of money deposited.</returns>
    [HttpGet("TotelOfMoneyDeposited")]
    [Authorize(Roles = "Student")]
    public IActionResult TotelOfMoneyDeposited()
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var deposits = unitOfWork.DepositionRepository.GetByFilter(d => d.UserId == userId);

            var transferToMe = unitOfWork.TransformationRepository.GetByFilter(t => t.ToUserId == userId);

            var totalOfMoneyDeposited = deposits.Sum(s => s.Balance);

            var total = totalOfMoneyDeposited + transferToMe.Sum(s => s.Balance);

            return Ok(new { Balance = total });
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }
}
