using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;

namespace GraduationProject.Controllers;

[Route("api/[controller]")]
[ApiController]
public class WalletsController(IWalletService walletService, IHttpContextAccessor _httpContextAccessor) : ControllerBase
{

    [HttpPost("Deposit")]
    [Authorize(Roles = "Student")]
    public IActionResult Deposit([FromForm] decimal balance)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var options = new SessionCreateOptions
            {
                SuccessUrl = $"{_httpContextAccessor.HttpContext.Request.Scheme}://{_httpContextAccessor.HttpContext.Request.Host}{_httpContextAccessor.HttpContext.Request.PathBase}/api/wallets/deposit/success?sessionId={{CHECKOUT_SESSION_ID}}&balance={balance}&userId={userId}",
                CancelUrl = $"{_httpContextAccessor.HttpContext.Request.Scheme}://{_httpContextAccessor.HttpContext.Request.Host}{_httpContextAccessor.HttpContext.Request.PathBase}/api/wallets/deposit/cancel?sessionId={{CHECKOUT_SESSION_ID}}",
                LineItems = new List<SessionLineItemOptions>
            {
                new SessionLineItemOptions
                {
                    Quantity = 1,
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)(balance * 100) ,
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = "Deposit",
                        },
                        Currency = "egp",
                    },
                },
            },
                Mode = "payment",
            };
            var service = new SessionService();
            var session = service.Create(options);
            var deposit = walletService.DepositAsync(userId, 0, session.Id);
            return Ok(new { Link = session.Url });
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }
    [HttpGet("Deposit/Success")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public IActionResult DepositSuccess(string sessionId, decimal balance, string userId)
    {
        try
        {
            var sessionService = new SessionService();
            var session = sessionService.Get(sessionId);
            if (session.PaymentStatus == "paid")
            {
                return Ok(new { Balance = walletService.DepositAsync(userId, balance, sessionId) });
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
    public async Task<IActionResult> TransferTo([FromForm][MaxLength(14)][MinLength(14)] string SSN,decimal balance)
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
}
