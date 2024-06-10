namespace Application.Services.Interfaces;

public interface IWalletService
{
    Task<decimal?> GetBalanceAsync(string userId);
    Task<decimal?> DepositAsync(string userId, decimal balance, string? sessionId);
    Task<object?> TransferAsync(string FromUser, string ToUserSSN, decimal balance, double? longit = null, double? latit = null);
}
