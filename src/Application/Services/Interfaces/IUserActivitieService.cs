using Application.DTOs;
using Domain.Entities;
using Microsoft.AspNetCore.Http;
using System.Linq.Expressions;

namespace Application.Services.Interfaces;

public interface IUserActivitieService
{
    IEnumerable<object> GetFeedbacks();
    Task AddFeedbackAsync(string feedback, string userId);
    Task<Dictionary<string, string>> UpdateProfileAsync(string userId, string? newFullName, IFormFile? picture);
    object? GetProfile(Expression<Func<User, bool>> filter);
    object? HistoryDonations(string userId);
    object GetHistoryTransactions(string userId);
    decimal? TotalOfMoneyPayed (string userId);
    decimal? TotalOfMoneyDeposited(string userId);
}
