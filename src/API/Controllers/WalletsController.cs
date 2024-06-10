using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Application.Services.Interfaces;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;

namespace GraduationProject.Controllers;

[Route("api/[controller]")]
[ApiController]
public class WalletsController(IWalletService walletService, IHttpContextAccessor _httpContextAccessor,StripeService stripeService) : ControllerBase
{

    [HttpPost("Deposit")]
    [Authorize(Roles = "Student")]
    public IActionResult Deposit([FromForm] decimal balance)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var successUrl = $"api/wallets/deposit/success?sessionId={{CHECKOUT_SESSION_ID}}&balance={balance}&userId={userId}";
            var cancelUrl = $"api/wallets/deposit/cancel";

            var PaymentLink = stripeService.CreatePaymentLinkAsync(balance, "Deposit", successUrl, cancelUrl);

            return Ok(new { Link = PaymentLink });
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }



    [HttpGet("Deposit/Success")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task<IActionResult> DepositSuccess(string sessionId, decimal balance, string userId)
    {
        try
        {
            var sessionService = new SessionService();
            var session = sessionService.Get(sessionId);
            if (session.PaymentStatus == "paid")
            {
                return Ok(new { Balance = await walletService.DepositAsync(userId, balance, sessionId) });
            }
            return BadRequest("Payment not completed");
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }


    [HttpGet("Deposit/Cancel")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public IActionResult DepositCancel(string sessionId)
    {
        return BadRequest("Payment canceled");
    }




    [HttpPost("Transfer_To")]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> TransferTo([MaxLength(14)][MinLength(14)] string SSN,decimal balance)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await walletService.TransferAsync(userId, SSN, balance);
            if (result is decimal)
                return Ok(new { Balance = result });
            return BadRequest(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }


    [HttpGet("GetBalance")]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> GetBalance()
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Ok(new { Balance = await walletService.GetBalanceAsync(userId) });
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }
}
