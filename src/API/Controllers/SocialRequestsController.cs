using Application.DTOs;
using Application.Services;
using Domain.Entities;
using Domain.IUnitOfWork;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;
using System.Security.Claims;

namespace API.Controllers;

/// <summary>
/// Controller to manage social requests.
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class SocialRequestsController(IUnitOfWork unitOfWork, PaymentHandlerService stripeService) : ControllerBase
{
    /// <summary>
    /// Adds a new social request.
    /// </summary>
    /// <param name="socialRequestDto"></param>
    /// <returns>Action result indicating the result of the operation.</returns>
    [HttpPost("Add")]
    [ValidateModel]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> PostSocialRequest([FromQuery] SocialRequestDto socialRequestDto)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var socialReqest = new SocialRequest
            {
                FatherName = socialRequestDto.FatherName,
                FatherIncome = socialRequestDto.FatherIncome,
                FatherIsDead = socialRequestDto.FatherIsDead,
                FatherJob = socialRequestDto.FatherJob,
                FatherState = socialRequestDto.FatherState,
                IsResidentInFamilyHome = socialRequestDto.IsResidentInFamilyHome,
                NumberOfFamilyMembers = socialRequestDto.NumberOfFamilyMembers,
                NumberOfFamilyMembersInEdu = socialRequestDto.NumberOfFamilyMembersInEdu,
                StudentId = userId,
                Status = "In Process",
                ServiceId = socialRequestDto.ServiceId
            };
            await unitOfWork.SocialRequestRepository.CreateAsync(socialReqest);
            await unitOfWork.SaveAsync();
            return Created();
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    /// <summary>
    /// Retrieves all social requests for the authenticated user.
    /// </summary>
    /// <returns>A list of social requests.</returns>
    [HttpGet("GetAll")]
    [Authorize(Roles = "Student,Donor,Moderator")]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var user = await unitOfWork.UserRepository.GetByIdAsync(userId);
            List<SocialRequest> socialRequest = new List<SocialRequest>();
            if (await unitOfWork.UserRepository.IsInRoleAsync(user, "Student"))
            {
                socialRequest = unitOfWork.SocialRequestRepository.GetByFilter(s => s.StudentId == userId, [i => i.Service]).ToList();
            }
            else if (await unitOfWork.UserRepository.IsInRoleAsync(user, "Donor"))
            {
                socialRequest = unitOfWork.SocialRequestRepository.GetByFilter(s => s.Status == "Accept", [i => i.Service]).ToList();
            }
            else if (await unitOfWork.UserRepository.IsInRoleAsync(user, "Moderator"))
            {
                socialRequest = unitOfWork.SocialRequestRepository.GetByFilter(s => s.Status == "In Process", [i => i.Service]).ToList();
            }
            return Ok(socialRequest.Select(selector => new
            {
                selector.Id,
                selector.Service.Name,
                selector.Service.Cost,
                selector.Service.Type,
                selector.Status
            }));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    /// <summary>
    /// Retrieves the specific details of a specific social request For Moderator.
    /// </summary>
    /// <param name="socialRequestId">The ID of the social request.</param>
    /// <returns>The specific details of the social request.</returns>
    [HttpGet("GetDetails_Moderator")]
    [Authorize(Roles = "Moderator")]
    public async Task<IActionResult> GetDetailsForModerator(int socialRequestId)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var user = await unitOfWork.UserRepository.GetByIdAsync(userId);

            var socialRequest = unitOfWork.SocialRequestRepository.GetByFilter(m => m.Id == socialRequestId, [n => n.Service, n => n.Student]).FirstOrDefault();

            return Ok(new
            {
                socialRequest.Student.FullName,
                socialRequest.FatherName,
                socialRequest.FatherIsDead,
                socialRequest.FatherIncome,
                socialRequest.FatherJob,
                socialRequest.FatherState,
                socialRequest.IsResidentInFamilyHome,
                socialRequest.NumberOfFamilyMembers,
                socialRequest.NumberOfFamilyMembersInEdu,
                socialRequest.Student.Squad_Year,
                socialRequest.Student.College_Name,
                socialRequest.Student.DateOfBirth,
                socialRequest.Service.Cost,
                socialRequest.Student.City,
                socialRequest.Student.State
            });


        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    /// <summary>
    /// Retrieves the specific details of a specific social request For Donor.
    /// </summary>
    /// <param name="socialRequestId">The ID of the social request.</param>
    /// <returns>The specific details of the social request.</returns>
    [HttpGet("GetDetails_Donor")]
    [Authorize(Roles = "Donor")]
    public async Task<IActionResult> GetDetailsForDonor(int socialRequestId)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var user = await unitOfWork.UserRepository.GetByIdAsync(userId);

            var socialRequest = unitOfWork.SocialRequestRepository.GetByFilter(m => m.Id == socialRequestId, [n => n.Service, n => n.Student]).FirstOrDefault();

            return Ok(new
            {
                socialRequest.FatherIsDead,
                socialRequest.FatherIncome,
                socialRequest.FatherJob,
                socialRequest.FatherState,
                socialRequest.IsResidentInFamilyHome,
                socialRequest.NumberOfFamilyMembers,
                socialRequest.NumberOfFamilyMembersInEdu,
                socialRequest.Student.Squad_Year,
                socialRequest.Student.College_Name,
                socialRequest.Student.DateOfBirth,
                socialRequest.Service.Cost,
                socialRequest.Student.City,
                socialRequest.Student.State
            });

        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    /// <summary>
    /// Accepts or rejects a social request.
    /// </summary>
    /// <param name="id">The ID of the social request.</param>
    /// <param name="status">The status to set (accepted or rejected).</param>
    /// <returns>Action result indicating the result of the operation.</returns>
    [HttpPut("Accept")]
    [Authorize(Roles = "Moderator")]
    public async Task<IActionResult> Accept(int id, bool status)
    {
        try
        {
            var socialRequest = await unitOfWork.SocialRequestRepository.GetByIdAsync(id);
            socialRequest.Status = status ? "Accept" : "Reject";
            unitOfWork.SocialRequestRepository.Update(socialRequest);
            await unitOfWork.SaveAsync();
            return Ok();
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

  

    /// <summary>
    /// Initiates a donation for a specific social request.
    /// </summary>
    /// <param name="socialRequestId">The ID of the social request to donate to.</param>
    /// <returns>A link to complete the donation.</returns>
    [HttpPut("DonateV1")]
    [Authorize(Roles = "Donor")]
    public IActionResult DonateV1(int socialRequestId)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var socialRequest = unitOfWork.SocialRequestRepository.GetByFilter(m => m.Id == socialRequestId, [i => i.Service]).FirstOrDefault();

            if (socialRequest is null)
                return NotFound();

            var successUrl = $"api/SocialRequests/Donate/success?sessionId={{CHECKOUT_SESSION_ID}}&socialRequestId={socialRequest.Id}&userId={userId}";
            var cancelUrl = $"api/SocialRequests/Donate/cancel?sessionId={{CHECKOUT_SESSION_ID}}";

            var PaymentLink = stripeService.CreatePaymentLink(socialRequest.Service.Cost, "Donate", successUrl, cancelUrl);

            return Ok(new { Link = PaymentLink });
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    /// <summary>
    /// Initiates a donation for a specific social request.
    /// </summary>
    /// <param name="socialRequestId">The ID of the social request to donate to.</param>
    /// <returns>A link to complete the donation.</returns>
    [HttpPut("DonateV2")]
    [Authorize(Roles = "Donor")]
    public async Task<IActionResult> DonateV2(int socialRequestId)
    {
        try
        {
            var socialRequest = unitOfWork.SocialRequestRepository.GetByFilter(m => m.Id == socialRequestId, [i => i.Service]).FirstOrDefault();

            if (socialRequest is null)
                return NotFound();

            var clientSecret = await stripeService.CreatepaymentIntent(socialRequest.Service.Cost);

            return Ok(new { client_secret = clientSecret });
        }
        catch (StripeException e)
        {
            return BadRequest(new { error = e.StripeError.Message });
        }
    }

    /// <summary>
    /// Initiates a donation for a specific social request.
    /// </summary>
    /// <param name="socialRequestId">The ID of the social request to donate to.</param>
    /// <returns>A link to complete the donation.</returns>
    [HttpPut("DonateV3")]
    [Authorize(Roles = "Donor")]
    public async Task<IActionResult> DonateV3(int socialRequestId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var socialRequest = unitOfWork.SocialRequestRepository.GetByFilter(s => s.Id == socialRequestId, [s => s.Service]).FirstOrDefault();
        if (socialRequest is null)
            return NotFound();
        using (var transaction = await unitOfWork.BeginTransactionAsync())
        {
            try
            {
                var donation = new Donation
                {
                    Date = DateTime.UtcNow,
                    SocialRequestId = socialRequest.Id,
                    UserId = userId
                };
                await unitOfWork.DonationRepository.CreateAsync(donation);
                await unitOfWork.SaveAsync();
                var donations = unitOfWork.AppWalletRepository.GetByFilter(a => a.Type == "Donations").FirstOrDefault();
                if (donations == null)
                {
                    var appWallet = new AppWallet
                    {
                        Balance = socialRequest.Service.Cost,
                        Type = "Donations"
                    };
                    await unitOfWork.AppWalletRepository.CreateAsync(appWallet);
                    await unitOfWork.SaveAsync();
                }
                else
                {
                    donations.Balance += socialRequest.Service.Cost;
                }
                socialRequest.Status = "Donated";
                socialRequest.DonationId = donation.Id;
                unitOfWork.SocialRequestRepository.Update(socialRequest);
                await unitOfWork.SaveAsync();
                transaction.Commit();
                return Ok(new { Message = "Donation done successful" });
            }
            catch (Exception e)
            {
                transaction.Rollback();
                return StatusCode(500, e.Message);
            }
        }
    }


    /// <summary>
    /// Handles successful donation callback.
    /// </summary>
    /// <param name="sessionId">The Stripe session ID.</param>
    /// <param name="socialRequestId">The ID of the social request.</param>
    /// <param name="userId">The ID of the user making the donation.</param>
    /// <returns>Action result indicating the result of the donation.</returns>
    [HttpGet("Donate/Success")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task<IActionResult> DonateSuccess(string sessionId, int socialRequestId, string userId)
    {
        var sessionService = new SessionService();
        var session = sessionService.Get(sessionId);
        if (session.PaymentStatus == "paid")
        {

            var socialRequest = unitOfWork.SocialRequestRepository.GetByFilter(s => s.Id == socialRequestId, [s => s.Service]).FirstOrDefault();
            using (var transaction = await unitOfWork.BeginTransactionAsync())
            {
                try
                {
                    var donation = new Donation
                    {
                        Date = DateTime.UtcNow,
                        SocialRequestId = socialRequest.Id,
                        UserId = userId
                    };
                    await unitOfWork.DonationRepository.CreateAsync(donation);
                    await unitOfWork.SaveAsync();
                    var donation1 = unitOfWork.AppWalletRepository.GetByFilter(a => a.Type == "Donations").FirstOrDefault();
                    if (donation1 == null)
                    {
                        var appWallet = new AppWallet
                        {
                            Balance = socialRequest.Service.Cost,
                            Type = "Donations"
                        };
                        await unitOfWork.AppWalletRepository.CreateAsync(appWallet);
                        await unitOfWork.SaveAsync();
                    }
                    else
                    {
                        donation1.Balance += socialRequest.Service.Cost;
                    }
                    socialRequest.Status = "Donated";
                    socialRequest.DonationId = donation.Id;
                    unitOfWork.SocialRequestRepository.Update(socialRequest);
                    await unitOfWork.SaveAsync();
                    transaction.Commit();
                    return Ok(new { Message = "Donation done successful" });
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    return StatusCode(500, e.Message);
                }
            }
        }
        return BadRequest("Payment not completed");
    }

    /// <summary>
    /// Handles donation cancellation callback.
    /// </summary>
    /// <returns>Action result indicating the payment was canceled.</returns>
    [HttpGet("Donate/Cancel")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public IActionResult DonateCancel()
    {
        return BadRequest("Payment canceled");
    }
}
