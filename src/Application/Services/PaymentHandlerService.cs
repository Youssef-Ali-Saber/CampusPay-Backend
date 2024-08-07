using Microsoft.AspNetCore.Http;
using Stripe;
using Stripe.Checkout;

namespace Application.Services;

public class PaymentHandlerService(IHttpContextAccessor httpContextAccessor)
{
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    public async Task<string> CreatepaymentIntent(decimal amount)
    {

        var paymentIntentOptions = new PaymentIntentCreateOptions
        {
            Amount = (long)(amount*100),
            Currency = "egp",
            PaymentMethodTypes = new List<string> { "card" },
            Metadata = new Dictionary<string, string>
            {
                { "integration_check", "accept_a_payment" }
            },
            CaptureMethod = "manual",
        };

        var paymentIntentService = new PaymentIntentService();
        var paymentIntent = await paymentIntentService.CreateAsync(paymentIntentOptions);

        return paymentIntent.ClientSecret;

    }
    public string CreatePaymentLink(decimal amount, string title, string successUrl, string cancelUrl)
    {
        var options = new SessionCreateOptions
        {
            SuccessUrl = $"{_httpContextAccessor.HttpContext.Request.Scheme}://{_httpContextAccessor.HttpContext.Request.Host}{_httpContextAccessor.HttpContext.Request.PathBase}/{successUrl}",
            CancelUrl = $"{_httpContextAccessor.HttpContext.Request.Scheme}://{_httpContextAccessor.HttpContext.Request.Host}{_httpContextAccessor.HttpContext.Request.PathBase}/{cancelUrl}",
            LineItems = new List<SessionLineItemOptions>
            {
                new SessionLineItemOptions
                {
                    Quantity = 1,
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)(amount * 100) ,
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = title,
                        },
                        Currency = "egp",
                    },
                },
            },
            Mode = "payment",
        };
        var service = new SessionService();

        var session = service.Create(options);

        return session.Url;

    }



}
