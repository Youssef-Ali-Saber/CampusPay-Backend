using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Application.DTOs;
using Application.Services;
using Domain.Entities;
using Domain.IUnitOfWork;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;

namespace API.Controllers;

/// <summary>
/// Controller to manage wallet operations.
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class WalletsController(
    WalletService walletService,
    PaymentHandlerService stripeService,
    HttpClient httpClient
    , IUnitOfWork unitOfWork
    )
    : ControllerBase
{

    
    
    /// <summary>
    /// Initiates a deposit to the user's wallet.
    /// </summary>
    /// <param name="balance">The amount to be deposited.</param>
    /// <returns>Payment link for the deposit.</returns>
    [HttpPost("DepositV1")]
    [Authorize(Roles = "Student")]
    public IActionResult DepositV1([FromQuery][Required] decimal balance)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var successUrl = $"api/wallets/deposit/success?sessionId={{CHECKOUT_SESSION_ID}}&balance={balance}&userId={userId}";
            var cancelUrl = $"api/wallets/deposit/cancel";

            var PaymentLink = stripeService.CreatePaymentLink(balance, "Deposit", successUrl, cancelUrl);
            return Ok(new { Link = PaymentLink });
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    /// <summary>
    /// Initiates a deposit to the user's wallet.
    /// </summary>
    /// <param name="balance">The amount to be deposited.</param>
    /// <returns>Payment link for the deposit.</returns>
    [HttpPost("DepositV2")]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> DepositV2([FromQuery][Required] decimal balance)
    {
        try
        {
            var clientSecret = await stripeService.CreatepaymentIntent(balance);
            return Ok(new { client_secret = clientSecret });
        }
        catch (StripeException e)
        {
            return BadRequest(new { error = e.StripeError.Message });
        }
    }

    /// <summary>
    /// Initiates a deposit to the user's wallet.
    /// </summary>
    /// <param name="balance">The amount to be deposited.</param>
    /// <returns>Payment link for the deposit.</returns>
    [HttpPost("DepositV3")]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> DepositV3([FromQuery][Required] decimal balance)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var user = await unitOfWork.UserRepository.GetByIdAsync(userId);
        using (var transaction = await unitOfWork.BeginTransactionAsync())
        {
            try
            {
                user.Balance += balance;
                unitOfWork.UserRepository.Update(user);
                var deposit = new Deposition
                {
                    Balance = balance,
                    Date = DateTime.UtcNow,
                    UserId = userId
                };
                await unitOfWork.DepositionRepository.CreateAsync(deposit);
                await unitOfWork.SaveAsync();
                transaction.Commit();
                return Ok(new { Balance = user.Balance });

            }
            catch (Exception e)
            {
                transaction.Rollback();
                return StatusCode(500, e.Message);
            }
        }
    }

    /// <summary>
    /// Handles successful deposit.
    /// </summary>
    /// <param name="sessionId">The Stripe session ID.</param>
    /// <param name="balance">The deposited amount.</param>
    /// <param name="userId">The ID of the user.</param>
    /// <returns>Updated balance after the deposit.</returns>
    [HttpGet("Deposit/Success")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task<IActionResult> DepositSuccess(string sessionId, decimal balance, string userId)
    {
        var sessionService = new SessionService();
        var session = sessionService.Get(sessionId);
        if (session.PaymentStatus == "paid")
        {
            var user = await unitOfWork.UserRepository.GetByIdAsync(userId);
            using (var transaction = await unitOfWork.BeginTransactionAsync())
            {
                try
                {
                    user.Balance += balance;
                    unitOfWork.UserRepository.Update(user);
                    var deposit = new Deposition
                    {
                        Balance = balance,
                        Date = DateTime.UtcNow,
                        UserId = userId,
                        SessionId = sessionId
                    };
                    await unitOfWork.DepositionRepository.CreateAsync(deposit);
                    await unitOfWork.SaveAsync();
                    transaction.Commit();
                    return Ok(new { Balance = user.Balance });

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
    /// Handles canceled deposit.
    /// </summary>
    /// <param name="sessionId">The Stripe session ID.</param>
    /// <returns>Indicates that the payment was canceled.</returns>
    [HttpGet("Deposit/Cancel")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public IActionResult DepositCancel(string sessionId)
    {
        return BadRequest("Payment canceled");
    }

    /// <summary>
    /// Transfers a specified amount to another user.
    /// </summary>
    /// <param name="SSN">The SSN of the recipient.</param>
    /// <param name="balance">The amount to be transferred.</param>
    /// <returns>Updated balance after the transfer or an error message.</returns>
    [HttpPost("Transfer_To")]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> TransferTo([MaxLength(14)][MinLength(14)] string SSN, decimal balance)
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

    /// <summary>
    /// Retrieves the current balance of the user's wallet.
    /// </summary>
    /// <returns>The current balance.</returns>
    [HttpGet("GetBalance")]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> GetBalance()
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await unitOfWork.UserRepository.GetByIdAsync(userId);
            return Ok(new { Balance = user?.Balance });
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }



    [HttpPost("FD_Model/Transfer")]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> TransferUsingMLModel([FromBody] MLModel_TransferDto transferDto)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var FromUser = await unitOfWork.UserRepository.GetByIdAsync(userId);
            var ToUser = unitOfWork.UserRepository.GetByFilter(u => u.SSN == transferDto.SSN).FirstOrDefault();
            if (FromUser?.Balance < (decimal)transferDto.Balance)
            {
                return BadRequest("Your balance not enough");
            }
            if (ToUser == null)
            {
                return BadRequest("Not found user have this SSN");
            }
            if (FromUser == ToUser)
            {
                return BadRequest("You can't transfer money to yourself");
            }
            var requestBody = new
            {
                trans_date_trans_time = "2022-05-05 08:48:20",
                dob = "2002-04-17",
                amt = transferDto.Balance,
                zip = FromUser.ZIPCode,
                city = FromUser.City,
                state = FromUser.State,
                user_lat = FromUser.Latitude,
                user_long = FromUser.Longitude,
                trans_lat = transferDto.Latitude,
                trans_long = transferDto.Longitude,
                gender = FromUser.Gender,
                transaction_type = "transfer money",
                email_sender = FromUser.Email,
                email_receiver = ToUser.Email
            };

            //{DateTime.Now.Year}-{DateTime.Now.Month}-{DateTime.Now.Day}
            //FromUser.DateOfBirth.ToString()
            //{DateTime.Now.Hour}:{DateTime.Now.Minute}:{DateTime.Now.Second}
            //var requestBody = new
            //{
            //    trans_date_trans_time = "2022-05-05 08:48:20",
            //    dob = "2001-01-25",
            //    amt = 100.0,
            //    zip = 65311,
            //    city = "fayoum",
            //    state = "FA",
            //    user_lat = 30.043489,
            //    user_long = 31.235291,
            //    trans_lat = Latitude,
            //    trans_long = 31.235292,
            //    gender = "M",
            //    transaction_type = "withdraw money",
            //    email_sender = "mah906@fayoum.edu.eg",
            //    email_receiver = "ah302@fayoum.edu.eg"
            //};

            // Serialize the request body to JSON
            string json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Send the POST request
            var response = await httpClient.PostAsync("https://fd-model-v7.onrender.com/predictapi", content);

            // Ensure the request was successful
            response.EnsureSuccessStatusCode();

            // Read the response content
            string responseBody = await response.Content.ReadAsStringAsync();

            // Deserialize the response content to an object
            var transactionResponse = JsonSerializer.Deserialize<Dictionary<string, string>>(responseBody);


            if (transactionResponse["prediction"] == "Not Fraud")
            {
                var result0 = await walletService.TransferAsync(userId, transferDto.SSN,(decimal)transferDto.Balance, transferDto.Longitude, transferDto.Latitude);
                if (result0 is decimal)
                    return Ok(new { YourBalanceNow = result0 });
                return BadRequest(result0);
            }
            return BadRequest("Fraud Transaction");
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }






}
