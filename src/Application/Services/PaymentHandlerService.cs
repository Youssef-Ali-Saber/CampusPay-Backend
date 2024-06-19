using Microsoft.AspNetCore.Http;
using Stripe.Checkout;

namespace Application.Services;

public class PaymentHandlerService(IHttpContextAccessor httpContextAccessor)
{
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

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
