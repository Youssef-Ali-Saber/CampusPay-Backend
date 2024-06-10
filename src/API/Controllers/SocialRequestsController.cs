using API;
using Application.DTOs;
using Application.Services.Interfaces;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;
using System.Security.Claims;

namespace GraduationProject.Controllers;

[Route("api/[controller]")]
[ApiController]
public class SocialRequestsController(ISocialRequestService socialRequestService, IHttpContextAccessor _httpContextAccessor,StripeService stripeService) : ControllerBase
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
        return Ok(socialRequestService.GetAllAsync(userId).Result?.Select(selector =>
        new
        {
            selector.Id,
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
            return Ok(socialRequestService.GetDetails(socialRequestId));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }


    [HttpPut("Accept")]
    [Authorize(Roles = "Moderator")]
    public async Task<IActionResult> Accept(int id, bool status)
    {
        try
        {
            await socialRequestService.AcceptAsync(id, status);
            return Ok();
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }


    [HttpPut("Donate")]
    [Authorize(Roles = "Donor")]
    public IActionResult Donate(int socialRequestId)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var socialRequest = socialRequestService.GetDetails(socialRequestId);
            var successUrl = $"api/SocialRequests/Donate/success?sessionId={{CHECKOUT_SESSION_ID}}&socialRequestId={socialRequest.Id}&userId={userId}";
            var cancelUrl = $"api/SocialRequests/Donate/cancel?sessionId={{CHECKOUT_SESSION_ID}}";

            var PaymentLink = stripeService.CreatePaymentLinkAsync(socialRequest.Service.Cost, "Donate", successUrl, cancelUrl);

            return Ok(new { Link = PaymentLink});
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }
    [HttpGet("Donate/Success")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task<IActionResult> DonateSuccess(string sessionId, int socialRequestId, string userId)
    {
        try
        {
            var sessionService = new SessionService();
            var session = sessionService.Get(sessionId);
            if (session.PaymentStatus == "paid")
            {
                return Ok(new { Message = await socialRequestService.DonateAsync(socialRequestId, userId) });
            }
            return BadRequest("Payment not completed");
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }
    [HttpGet("Donate/Cancel")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public IActionResult DonateCancel()
    {
        return BadRequest("Payment canceled");
    }

}
